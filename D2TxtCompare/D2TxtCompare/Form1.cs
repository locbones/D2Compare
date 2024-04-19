using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace D2TxtCompare
{
    public partial class Form1 : Form
    {
        string sourceFolderPath = "";
        string targetFolderPath = "";
        string sourceFolderPathC = "";
        string targetFolderPathC = "";

        public Form1()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.compare_yRb_icon;
        }

        #region Parsing Functions

        private void CompareFiles(string sourcePath, string targetPath)
        {

            //Parse results from ReadCSV function and identify headers
            Dictionary<string, List<string>> sourceData = ReadCSV(sourcePath);
            Dictionary<string, List<string>> targetData = ReadCSV(targetPath);

            var allHeaders = new HashSet<string>(sourceData.Keys);
            allHeaders.UnionWith(targetData.Keys);

            string rowHeaderColumn = allHeaders.FirstOrDefault(header => sourceData.ContainsKey(header) && targetData.ContainsKey(header));
            if (rowHeaderColumn == null)
            {
                Debug.WriteLine("Row header column not found in both files.");
                return;
            }

            //Compare dictionary keys to find modified columns
            var addedColumns = targetData.Keys.Except(sourceData.Keys);
            var removedColumns = sourceData.Keys.Except(targetData.Keys);

            ShowColumnHeaderDifferences(addedColumns, removedColumns, sourcePath);

            //Compare dictionary keys to find modified rows
            var addedRowsTask = Task.Run(() => targetData[rowHeaderColumn].Except(sourceData[rowHeaderColumn]));
            var removedRowsTask = Task.Run(() => sourceData[rowHeaderColumn].Except(targetData[rowHeaderColumn]));
            var addedRows = addedRowsTask.Result;
            var removedRows = GetRemovedRows(sourceData, targetData, rowHeaderColumn);
            var allRemovedRows = GetListFromDictionary(removedRows);

            Task.WaitAll(addedRowsTask, removedRowsTask);
            ShowRowHeaderDifferences(addedRows.ToList(), allRemovedRows, sourcePath);

            //Group changes by the row header
            var groupedDifferences = GetGroupedDifferences(sourceData, targetData, allHeaders, rowHeaderColumn);

            if (groupedDifferences.Any())
            {
                string formattedRtf = FormatRtf(groupedDifferences, sourcePath);
                textValues.Clear();
                textValues.SelectedRtf = formattedRtf;
            }
            else
                textValues.Text = "No differences found.";
        }

        //Parse CSV files from dropdown
        private Dictionary<string, List<string>> ReadCSV(string filePath)
        {
            Dictionary<string, List<string>> data = new Dictionary<string, List<string>>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                string[] headers = reader.ReadLine().Split('\t');

                foreach (var header in headers)
                    data[header] = new List<string>();

                while (!reader.EndOfStream)
                {
                    string[] values = reader.ReadLine().Split('\t');

                    for (int i = 0; i < headers.Length; i++)
                        data[headers[i]].Add(values[i]);
                }
            }

            return data;
        }

        //Function to allow multiple identical entries in the removedRows list (unused entries)
        private Dictionary<string, int> GetRemovedRows(Dictionary<string, List<string>> file1Data, Dictionary<string, List<string>> file2Data, string rowHeaderColumn)
        {
            var removedRows = new Dictionary<string, int>();
            foreach (var row in file1Data[rowHeaderColumn])
            {
                if (removedRows.ContainsKey(row))
                    removedRows[row]++;
                else
                    removedRows[row] = 1;
            }
            foreach (var row in file2Data[rowHeaderColumn])
            {
                if (removedRows.ContainsKey(row))
                {
                    removedRows[row]--;
                    if (removedRows[row] == 0)
                        removedRows.Remove(row);
                }
            }
            return removedRows;
        }

        private List<string> GetListFromDictionary(Dictionary<string, int> dictionary)
        {
            var list = new List<string>();
            foreach (var kvp in dictionary)
            {
                for (int i = 0; i < kvp.Value; i++)
                    list.Add(kvp.Key);
            }
            return list;
        }

        //Apply parallel task to analyze and output grouped differences for row changes
        private List<(string, List<string>)> GetGroupedDifferences(Dictionary<string, List<string>> file1Data, Dictionary<string, List<string>> file2Data, HashSet<string> allHeaders, string rowHeaderColumn)
        {
            var groupedDifferences = new Dictionary<string, List<(string, string)>>();

            Parallel.ForEach(allHeaders, header =>
            {
                if (!file1Data.ContainsKey(header) || !file2Data.ContainsKey(header))
                    return;

                var file1Column = file1Data[header];
                var file2Column = file2Data[header];

                int minLength = Math.Min(file1Column.Count, file2Column.Count);
                for (int i = 0; i < minLength; i++)
                {
                    if (file1Column[i] != file2Column[i])
                    {
                        string rowHeader = file2Data[rowHeaderColumn][i];
                        string valueDifference = $"<b>{header}</b>: '{file1Column[i]}' -> '{file2Column[i]}'";
                        string column0Value = $"{file2Data[rowHeaderColumn][i]} (Row {i + 1})";
                        int columnIndex = allHeaders.ToList().IndexOf(header);

                        //Group all changes under the Column 0 value
                        lock (groupedDifferences)
                        {
                            if (!groupedDifferences.ContainsKey(column0Value))
                                groupedDifferences[column0Value] = new List<(string, string)>();

                            groupedDifferences[column0Value].Add((valueDifference, columnIndex.ToString()));
                        }
                    }
                }
            });

            // Sort the groups by row index and then by column index
            var sortedGroups = groupedDifferences.OrderBy(pair => int.Parse(Regex.Match(pair.Key, @"Row (\d+)").Groups[1].Value))
                                                  .Select(pair =>
                                                  {
                                                      var sortedDifferences = pair.Value.OrderBy(t => int.Parse(t.Item2)).ToList();
                                                      return (pair.Key, sortedDifferences.Select(t => t.Item1).ToList());
                                                  }).ToList();
            return sortedGroups;
        }

        #endregion

        #region Display Functions

        private void ShowColumnHeaderDifferences(IEnumerable<string> addedColumns, IEnumerable<string> removedColumns, string fileType)
        {
            textColumns.Clear();

            var changedHeaders = new List<string>();

            // Identify added and removed columns to perform manual value fixes
            foreach (var addedColumn in addedColumns.ToList())
            {
                foreach (var removedColumn in removedColumns.ToList())
                {
                    if (ApplyManualFixes(addedColumn, removedColumn, fileType))
                    {
                        var oldNewPair = $"{removedColumn} -> {addedColumn}";
                        changedHeaders.Add(oldNewPair);
                        addedColumns = addedColumns.Except(new[] { addedColumn });
                        removedColumns = removedColumns.Except(new[] { removedColumn });
                        break;
                    }
                }
            }

            // If the number of added/removed entries are the same, then they have been changed instead
            if (addedColumns.Count() == removedColumns.Count())
            {
                var addedAndRemovedHeaders = addedColumns.Zip(removedColumns, (added, removed) => $"{removed} -> {added}");

                foreach (var headerPair in addedAndRemovedHeaders)
                {
                    textColumns.SelectionColor = Color.MidnightBlue;
                    textColumns.SelectionFont = new Font(textColumns.Font, FontStyle.Bold);
                    textColumns.AppendText($"Changed: ");
                    textColumns.SelectionColor = textColumns.ForeColor;
                    textColumns.SelectionFont = new Font(textColumns.Font, FontStyle.Regular);
                    textColumns.AppendText($"{headerPair}\n");
                }

                // Remove changed entries from addedColumns and removedColumns
                addedColumns = addedColumns.Except(addedAndRemovedHeaders.Select(pair => pair.Split(" -> ")[1]));
                removedColumns = removedColumns.Except(addedAndRemovedHeaders.Select(pair => pair.Split(" -> ")[0]));
            }

            // Show changed entries
            foreach (var headerPair in changedHeaders)
            {
                textColumns.SelectionColor = Color.MidnightBlue;
                textColumns.SelectionFont = new Font(textColumns.Font, FontStyle.Bold);
                textColumns.AppendText($"Changed: ");
                textColumns.SelectionColor = textColumns.ForeColor;
                textColumns.SelectionFont = new Font(textColumns.Font, FontStyle.Regular);
                textColumns.AppendText($"{headerPair}\n");
            }

            // Show added entries
            foreach (var column in addedColumns)
            {
                textColumns.SelectionColor = Color.Green;
                textColumns.SelectionFont = new Font(textColumns.Font, FontStyle.Bold);
                textColumns.AppendText($"Added: ");
                textColumns.SelectionColor = textColumns.ForeColor;
                textColumns.SelectionFont = new Font(textColumns.Font, FontStyle.Regular);
                textColumns.AppendText($"{column}\n");
            }

            // Show removed entries
            foreach (var column in removedColumns)
            {
                textColumns.SelectionColor = Color.Red;
                textColumns.SelectionFont = new Font(textColumns.Font, FontStyle.Bold);
                textColumns.AppendText($"Removed: ");
                textColumns.SelectionColor = textColumns.ForeColor;
                textColumns.SelectionFont = new Font(textColumns.Font, FontStyle.Regular);
                textColumns.AppendText($"{column}\n");
            }
        }

        private void ShowRowHeaderDifferences(IEnumerable<string> addedRows, IEnumerable<string> removedRows, string fileType)
        {
            textRows.Clear();

            var processedAddedRows = new HashSet<string>();
            var processedRemovedRows = new HashSet<string>();

            var changedHeaders = new List<string>();

            // Check for rows where the only difference is an asterisk or special conditions
            foreach (var addedRow in addedRows)
            {
                foreach (var removedRow in removedRows)
                {
                    if (!processedAddedRows.Contains(addedRow) && !processedRemovedRows.Contains(removedRow))
                    {
                        if (ApplyManualFixes(addedRow, removedRow, fileType))
                        {
                            var oldNewPair = $"{removedRow} -> {addedRow}";
                            changedHeaders.Add(oldNewPair);
                            processedAddedRows.Add(addedRow);
                            processedRemovedRows.Add(removedRow);
                        }
                    }
                }
            }

            // Show changed entries if the number of added and removed rows are the same
            if (addedRows.Count() == removedRows.Count())
            {
                var addedAndRemovedHeaders = addedRows.Zip(removedRows, (added, removed) => $"{removed} -> {added}");

                foreach (var headerPair in addedAndRemovedHeaders)
                {
                    textRows.SelectionColor = Color.MidnightBlue;
                    textRows.SelectionFont = new Font(textRows.Font, FontStyle.Bold);
                    textRows.AppendText($"Changed: ");
                    textRows.SelectionColor = textRows.ForeColor;
                    textRows.SelectionFont = new Font(textRows.Font, FontStyle.Regular);
                    textRows.AppendText($"{headerPair}\n");
                }

                // Remove changed entries from processedAddedRows and processedRemovedRows
                foreach (var pair in addedAndRemovedHeaders)
                {
                    var parts = pair.Split(" -> ");
                    processedAddedRows.Add(parts[1]);
                    processedRemovedRows.Add(parts[0]);
                }
            }

            // Show changed entries where the only difference is an asterisk or special conditions
            foreach (var headerPair in changedHeaders)
            {
                textRows.SelectionColor = Color.MidnightBlue;
                textRows.SelectionFont = new Font(textRows.Font, FontStyle.Bold);
                textRows.AppendText($"Changed: ");
                textRows.SelectionColor = textRows.ForeColor;
                textRows.SelectionFont = new Font(textRows.Font, FontStyle.Regular);
                textRows.AppendText($"{headerPair}\n");
            }

            // Show added entries
            foreach (var row in addedRows.Where(r => !processedAddedRows.Contains(r)))
            {
                textRows.SelectionColor = Color.Green;
                textRows.SelectionFont = new Font(textRows.Font, FontStyle.Bold);
                textRows.AppendText($"Added: ");
                textRows.SelectionColor = textRows.ForeColor;
                textRows.SelectionFont = new Font(textRows.Font, FontStyle.Regular);
                textRows.AppendText($"{row}\n");
            }

            // Show removed entries individually
            foreach (var removedRow in removedRows.Where(r => !processedRemovedRows.Contains(r)))
            {
                textRows.SelectionColor = Color.Red;
                textRows.SelectionFont = new Font(textRows.Font, FontStyle.Bold);
                textRows.AppendText($"Removed: ");
                textRows.SelectionColor = textRows.ForeColor;
                textRows.SelectionFont = new Font(textRows.Font, FontStyle.Regular);
                textRows.AppendText($"{removedRow}\n");
                processedRemovedRows.Add(removedRow); // Mark as processed
            }
        }

        private bool ApplyManualFixes(string addedHeader, string removedHeader, string fileType)
        {
            // Check if the columns were converted to comment columns
            if (addedHeader.ToLower().Replace("*", "") == removedHeader.ToLower().Replace("*", ""))
                return true;

            // Check for npc typo/name changes
            if ((addedHeader.IndexOf("hratli", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("hralti", StringComparison.OrdinalIgnoreCase) != -1) || (addedHeader.IndexOf("anya", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("drehya", StringComparison.OrdinalIgnoreCase) != -1))
                return true;

            // Cubemain hotfix
            if ((addedHeader.IndexOf("firstLadderSeason", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("ladder", StringComparison.OrdinalIgnoreCase) != -1))
                return true;

            // DifficultyLevels hotfix
            if ((addedHeader.IndexOf("MercenaryDamagePercentVSBoss", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("HireableBossDamagePercent", StringComparison.OrdinalIgnoreCase) != -1))
                return true;

            // Special condition for "itemstatcost" file
            List<string> addedHeaders = new List<string> { "lasthitreactframe", "create_season", "bonus_mindamage", "bonus_maxdamage", "item_pierce_cold_immunity", "item_pierce_fire_immunity", "item_pierce_light_immunity", "item_pierce_poison_immunity", "item_pierce_damage_immunity", "item_pierce_magic_immunity", "item_charge_noconsume", "modifierlist_castid", "item_noconsume", "passive_mastery_noconsume", "passive_mastery_replenish_oncrit", "passive_mastery_gethit_rate", "passive_mastery_attack_speed" };
            List<string> removedHeaders = new List<string> { "unused183", "unused184", "unused185", "unused186", "unused187", "unused189", "unused190", "unused191", "unused192", "unused193", "unused200", "unused202", "unused204", "unused205", "unused206", "unused207", "unused212" };

            if ((addedHeaders.Contains(addedHeader, StringComparer.OrdinalIgnoreCase) && removedHeaders.Contains(removedHeader, StringComparer.OrdinalIgnoreCase)) || (addedHeaders.Contains(removedHeader, StringComparer.OrdinalIgnoreCase) && removedHeaders.Contains(addedHeader, StringComparer.OrdinalIgnoreCase)))
                return true;

            // Itemtypes hotfix
            if ((addedHeader.IndexOf("MaxSockets1", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("MaxSock1", StringComparison.OrdinalIgnoreCase) != -1) || (addedHeader.IndexOf("MaxSockets2", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("MaxSock25", StringComparison.OrdinalIgnoreCase) != -1) || (addedHeader.IndexOf("MaxSockets3", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("MaxSock40", StringComparison.OrdinalIgnoreCase) != -1) || (addedHeader.IndexOf("Any", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("None", StringComparison.OrdinalIgnoreCase) != -1))
                return true;

            // Levels hotfix
            if ((addedHeader.IndexOf("MonLvl", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("MonLvl1", StringComparison.OrdinalIgnoreCase) != -1) || (addedHeader.IndexOf("MonLvl(N)", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("MonLvl2", StringComparison.OrdinalIgnoreCase) != -1) || (addedHeader.IndexOf("MonLvl(H)", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("MonLvl3", StringComparison.OrdinalIgnoreCase) != -1) || (addedHeader.IndexOf("MonLvlEx", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("MonLvl1Ex", StringComparison.OrdinalIgnoreCase) != -1) || (addedHeader.IndexOf("MonLvlEx(N)", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("MonLvl2Ex", StringComparison.OrdinalIgnoreCase) != -1) || (addedHeader.IndexOf("MonLvlEx(H)", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("MonLvl3Ex", StringComparison.OrdinalIgnoreCase) != -1))
                return true;

            // Missiles hotfix
            if ((addedHeader.IndexOf("nihlathakcontrol", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("nehlithakcontrol", StringComparison.OrdinalIgnoreCase) != -1) || (addedHeader.IndexOf("nihlathakswoosh", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("nehlithakswoosh", StringComparison.OrdinalIgnoreCase) != -1) || (addedHeader.IndexOf("nihlathakdebris1", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("nehlithakdebris1", StringComparison.OrdinalIgnoreCase) != -1) || (addedHeader.IndexOf("nihlathakdebris2", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("nehlithakdebris2", StringComparison.OrdinalIgnoreCase) != -1) || (addedHeader.IndexOf("nihlathakdebris3", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("nehlithakdebris3", StringComparison.OrdinalIgnoreCase) != -1) || (addedHeader.IndexOf("nihlathakdebris4", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("nehlithakdebris4", StringComparison.OrdinalIgnoreCase) != -1)
                 || (addedHeader.IndexOf("nihlathakglow", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("nehlithakglow", StringComparison.OrdinalIgnoreCase) != -1) || (addedHeader.IndexOf("nihlathakhole", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("nehlithakhole", StringComparison.OrdinalIgnoreCase) != -1) || (addedHeader.IndexOf("nihlathakholelight", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("nehlithakholelight", StringComparison.OrdinalIgnoreCase) != -1) || (addedHeader.IndexOf("nihlathakglow2", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("nehlithakglow2", StringComparison.OrdinalIgnoreCase) != -1) || (addedHeader.IndexOf("nihlathakbonechips", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("nehlithakbonechips", StringComparison.OrdinalIgnoreCase) != -1))
                return true;

            // Monstats hotfix
            bool changed = false;

            Dictionary<string, string> mappings = new Dictionary<string, string> {
                { "ShieldBlockOverride", "NoShldBlock" },
                { "TreasureClass", "TreasureClass1" },
                { "TreasureClassChamp", "TreasureClass2" },
                { "TreasureClassUnique", "TreasureClass3" },
                { "TreasureClassQuest", "TreasureClass4" },
                { "TreasureClass(N)", "TreasureClass1(N)" },
                { "TreasureClassChamp(N)", "TreasureClass2(N)" },
                { "TreasureClassUnique(N)", "TreasureClass3(N)" },
                { "TreasureClassQuest(N)", "TreasureClass4(N)" },
                { "TreasureClass(H)", "TreasureClass1(H)" },
                { "TreasureClassChamp(H)", "TreasureClass2(H)" },
                { "TreasureClassUnique(H)", "TreasureClass3(H)" },
                { "TreasureClassQuest(H)", "TreasureClass4(H)" }
            };

            foreach (var mapping in mappings)
            {
                if ((addedHeader.Equals(mapping.Key, StringComparison.OrdinalIgnoreCase) &&
                     removedHeader.Equals(mapping.Value, StringComparison.OrdinalIgnoreCase)) ||
                    (addedHeader.Equals(mapping.Value, StringComparison.OrdinalIgnoreCase) &&
                     removedHeader.Equals(mapping.Key, StringComparison.OrdinalIgnoreCase)))
                {
                    changed = true;
                    break;
                }
            }

            if (changed)
                return true;

            //Objects hotfix
            if ((addedHeader.IndexOf("*Description", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("description - not loaded", StringComparison.OrdinalIgnoreCase) != -1))
                return true;

            //Runes hotfix
            if ((addedHeader.IndexOf("*RunesUsed", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("*runes", StringComparison.OrdinalIgnoreCase) != -1) || (addedHeader.IndexOf("firstLadderSeason", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("server", StringComparison.OrdinalIgnoreCase) != -1))
                return true;

            //Setitems hotfix
            if ((addedHeader.IndexOf("*ItemName", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("*item", StringComparison.OrdinalIgnoreCase) != -1))
                return true;

            //Shrines hotfix
            if ((addedHeader.IndexOf("Name", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("Shrine Type", StringComparison.OrdinalIgnoreCase) != -1) || (addedHeader.IndexOf("*Shrine Type", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("Shrine name", StringComparison.OrdinalIgnoreCase) != -1) || (addedHeader.IndexOf("*Effect", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("Effect", StringComparison.OrdinalIgnoreCase) != -1))
                return true;

            //TreasureClassEx hotfix
            if ((addedHeader.IndexOf("*ItemProbSum", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("SumItems", StringComparison.OrdinalIgnoreCase) != -1) || (addedHeader.IndexOf("*ItemProbTotal", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("TotalProb", StringComparison.OrdinalIgnoreCase) != -1) || (addedHeader.IndexOf("*TreasureClassDropChance", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("DropChance", StringComparison.OrdinalIgnoreCase) != -1) || (addedHeader.IndexOf("*eol", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("Term", StringComparison.OrdinalIgnoreCase) != -1))
                return true;

            //UniqueItems hotfix
            if ((addedHeader.IndexOf("*ItemName", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("*type", StringComparison.OrdinalIgnoreCase) != -1) || (addedHeader.IndexOf("*Shrine Type", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("Shrine name", StringComparison.OrdinalIgnoreCase) != -1) || (addedHeader.IndexOf("*Effect", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("Effect", StringComparison.OrdinalIgnoreCase) != -1))
                return true;

            //Weapons hotfix
            if ((addedHeader.IndexOf("*comment", StringComparison.OrdinalIgnoreCase) != -1 && removedHeader.IndexOf("special", StringComparison.OrdinalIgnoreCase) != -1))
                return true;

            return false;
        }

        //Apply Color and Font formatting to outputs
        private string FormatRtf(List<(string, List<string>)> groupedDifferences, string fileName)
        {
            string rtf = @"{\rtf1\ansi\deff0{\colortbl ;\red0\green0\blue128;}\f0";
            bool isFirstGroup = true; // Flag to track the first group

            foreach (var kvp in groupedDifferences)
            {
                // Add a newline if it's not the first group
                if (!isFirstGroup)
                    rtf += "\\par ";

                rtf += $"{{\\cf1\\b {kvp.Item1}\\b0}}"; // Applying midnightblue color to the header
                rtf += "\\cf0"; // Reset color to black

                // List all differences for the current column 0 value
                foreach (var diff in kvp.Item2)
                {
                    rtf += "\\par ";
                    rtf += $"- {diff.Replace("<b>", @"\b ").Replace("</b>", @"\b0 ")}";
                }

                // Set the flag to false after the first group is processed
                isFirstGroup = false;
                rtf += "\\par "; // Add a blank line after each group
            }
            rtf += "}";
            return rtf;
        }

        #endregion

        #region Index Controls

        //User changed file to compare
        private void dropFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            sourceFolderPathC = Path.Combine(sourceFolderPath, dropFiles.Text);
            targetFolderPathC = Path.Combine(targetFolderPath, dropFiles.Text);
            CompareFiles(sourceFolderPathC, targetFolderPathC);
        }

        //User changed source folder location
        private void dropSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dropSource.SelectedIndex == 0)
                sourceFolderPath = @"TXT\113c";
            if (dropSource.SelectedIndex == 1)
                sourceFolderPath = @"TXT\62115";
            if (dropSource.SelectedIndex == 2)
                sourceFolderPath = @"TXT\64954";
            if (dropSource.SelectedIndex == 3)
                sourceFolderPath = @"TXT\65890";
            if (dropSource.SelectedIndex == 4)
                sourceFolderPath = @"TXT\67314";
            if (dropSource.SelectedIndex == 5)
                sourceFolderPath = @"TXT\67358";
            if (dropSource.SelectedIndex == 6)
                sourceFolderPath = @"TXT\67554";
            if (dropSource.SelectedIndex == 7)
                sourceFolderPath = @"TXT\68992";
            if (dropSource.SelectedIndex == 8)
                sourceFolderPath = @"TXT\69270";
            if (dropSource.SelectedIndex == 9)
                sourceFolderPath = @"TXT\70161";
            if (dropSource.SelectedIndex == 10)
                sourceFolderPath = @"TXT\71336";
            if (dropSource.SelectedIndex == 11)
                sourceFolderPath = @"TXT\71510";
            if (dropSource.SelectedIndex == 12)
                sourceFolderPath = @"TXT\71776";
            if (dropSource.SelectedIndex == 13)
                sourceFolderPath = @"TXT\73090";
            if (dropSource.SelectedIndex == 14)
                sourceFolderPath = @"TXT\77312";
            if (dropSource.SelectedIndex == 15 && string.IsNullOrEmpty(sourceFolderPath))
            {
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    sourceFolderPath = folderBrowserDialog.SelectedPath;

                    if (dropSource.Items.Count > 0)
                    {
                        dropSource.Items[dropSource.Items.Count - 1] = "Custom (Loaded)"; //Update text to display loaded status
                        dropSource.SelectedIndex = dropSource.Items.Count - 1;
                    }
                    else
                        dropSource.Items.Add("Custom (Loaded)");
                }
            }
        }

        //User changed target folder location
        private void dropTarget_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dropTarget.SelectedIndex == 0)
                targetFolderPath = @"TXT\113c";
            if (dropTarget.SelectedIndex == 1)
                targetFolderPath = @"TXT\62115";
            if (dropTarget.SelectedIndex == 2)
                targetFolderPath = @"TXT\64954";
            if (dropTarget.SelectedIndex == 3)
                targetFolderPath = @"TXT\65890";
            if (dropTarget.SelectedIndex == 4)
                targetFolderPath = @"TXT\67314";
            if (dropTarget.SelectedIndex == 5)
                targetFolderPath = @"TXT\67358";
            if (dropTarget.SelectedIndex == 6)
                targetFolderPath = @"TXT\67554";
            if (dropTarget.SelectedIndex == 7)
                targetFolderPath = @"TXT\68992";
            if (dropTarget.SelectedIndex == 8)
                targetFolderPath = @"TXT\69270";
            if (dropTarget.SelectedIndex == 9)
                targetFolderPath = @"TXT\70161";
            if (dropTarget.SelectedIndex == 10)
                targetFolderPath = @"TXT\71336";
            if (dropTarget.SelectedIndex == 11)
                targetFolderPath = @"TXT\71510";
            if (dropTarget.SelectedIndex == 12)
                targetFolderPath = @"TXT\71776";
            if (dropTarget.SelectedIndex == 13)
                targetFolderPath = @"TXT\73090";
            if (dropTarget.SelectedIndex == 14)
                targetFolderPath = @"TXT\77312";
            if (dropTarget.SelectedIndex == 15 && string.IsNullOrEmpty(targetFolderPath))
            {
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    targetFolderPath = folderBrowserDialog.SelectedPath;

                    if (dropTarget.Items.Count > 0)
                    {
                        dropTarget.Items[dropTarget.Items.Count - 1] = "Custom (Loaded)"; //Update text to display loaded status
                        dropTarget.SelectedIndex = dropTarget.Items.Count - 1; 
                    }
                    else
                        dropTarget.Items.Add("Custom (Loaded)");
                }
            }

            dropFiles.Items.Clear();
            PopulateComboBox(sourceFolderPath, targetFolderPath, dropFiles);
            UpdateRichTextBox(sourceFolderPath, targetFolderPath, textFiles);
            CompareFiles(sourceFolderPathC, targetFolderPathC);
        }

        //Show all files that exist in both comparison folders
        private void PopulateComboBox(string sourcePath, string directoryPath, System.Windows.Forms.ComboBox comboBox)
        {
            if (Directory.Exists(directoryPath))
            {
                string[] sFilenames = Directory.GetFiles(sourcePath);
                string[] tFilenames = Directory.GetFiles(directoryPath);

                foreach (string file1 in sFilenames)
                {
                    foreach (string file2 in tFilenames)
                    {
                        if (string.Equals(Path.GetFileName(file1), Path.GetFileName(file2), StringComparison.OrdinalIgnoreCase))
                            comboBox.Items.Add(Path.GetFileName(file1));
                    }
                }

                if (comboBox.Items.Count > 0)
                    comboBox.SelectedIndex = 0;
            }
            else
                Debug.WriteLine("Directory does not exist: " + directoryPath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //Show all files that have been added/removed between comparison folders
        private void UpdateRichTextBox(string sourcePath, string directoryPath, System.Windows.Forms.RichTextBox richTextBox)
        {
            if (Directory.Exists(sourcePath) && Directory.Exists(directoryPath))
            {
                string[] sFilenames = Directory.GetFiles(sourcePath);
                string[] tFilenames = Directory.GetFiles(directoryPath);

                List<string> filesOnlyInSource = new List<string>();
                List<string> filesOnlyInTarget = new List<string>();

                // Find all matching files in source location
                foreach (string file1 in sFilenames)
                {
                    string fileName1 = Path.GetFileName(file1);
                    bool found = false;
                    foreach (string file2 in tFilenames)
                    {
                        string fileName2 = Path.GetFileName(file2);
                        if (string.Equals(fileName1, fileName2, StringComparison.OrdinalIgnoreCase))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        filesOnlyInSource.Add(fileName1); //Add to found list
                    }
                }

                // Find all matching files in target location
                foreach (string file2 in tFilenames)
                {
                    string fileName2 = Path.GetFileName(file2);
                    bool found = false;
                    foreach (string file1 in sFilenames)
                    {
                        string fileName1 = Path.GetFileName(file1);
                        if (string.Equals(fileName1, fileName2, StringComparison.OrdinalIgnoreCase))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        filesOnlyInTarget.Add(fileName2); //Add to not found list
                    }
                }

                // Set the initial font and color
                richTextBox.SelectionFont = new Font(richTextBox.Font, FontStyle.Regular);
                richTextBox.SelectionColor = richTextBox.ForeColor;

                // Removed Files
                foreach (string fileName in filesOnlyInSource)
                {
                    richTextBox.SelectionColor = Color.Red;
                    richTextBox.SelectionFont = new Font(richTextBox.Font, FontStyle.Bold);
                    richTextBox.AppendText($"Removed: ");
                    richTextBox.SelectionColor = richTextBox.ForeColor;
                    richTextBox.SelectionFont = new Font(richTextBox.Font, FontStyle.Regular);
                    richTextBox.AppendText(fileName + Environment.NewLine);
                }

                // Added Files
                foreach (string fileName in filesOnlyInTarget)
                {
                    richTextBox.SelectionColor = Color.Green;
                    richTextBox.SelectionFont = new Font(richTextBox.Font, FontStyle.Bold);
                    richTextBox.AppendText($"Added: ");
                    richTextBox.SelectionColor = richTextBox.ForeColor;
                    richTextBox.SelectionFont = new Font(richTextBox.Font, FontStyle.Regular);
                    richTextBox.AppendText(fileName + Environment.NewLine);
                }
            }
        }

        #endregion

        #region Search Functions

        //Initial highlight option called from selectedIndexChanged
        private void HighlightWord(string word)
        {
            int index = textValues.Text.IndexOf(word, currentSearchIndex, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                textValues.SelectionStart = index;
                textValues.SelectionLength = word.Length;
                textValues.SelectionBackColor = Color.Yellow;
                textValues.ScrollToCaret();
            }
        }

        private int currentSearchIndex = 0;
        private int totalMatches = 0;
        private int currentMatchIndex = 0;

        private void HighlightNextMatch(string word)
        {
            // Clear previous selection and reset background color
            textValues.SelectionStart = 1;
            textValues.SelectionLength = textValues.Text.Length;
            textValues.SelectionBackColor = textValues.BackColor;

            //Adjust seek position to assume first match is already selected
            if (currentSearchIndex == 0)
                currentSearchIndex = textValues.Text.IndexOf(word, currentSearchIndex, StringComparison.OrdinalIgnoreCase) + word.Length;
            int index = textValues.Text.IndexOf(word, currentSearchIndex, StringComparison.OrdinalIgnoreCase);

            if (index >= 0)
            {
                textValues.Select(index, word.Length);
                textValues.SelectionBackColor = Color.Yellow;
                textValues.ScrollToCaret();
                currentSearchIndex = index + word.Length;
                currentMatchIndex++;
                UpdateMatchLabel();
            }
            else
            {
                // If no more matches found, wrap around and highlight the first occurrence
                index = textValues.Text.IndexOf(word, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    textValues.Select(index, word.Length);
                    textValues.SelectionBackColor = Color.Yellow;
                    textValues.ScrollToCaret();
                    currentSearchIndex = index + word.Length;
                    currentMatchIndex = 1;
                    UpdateMatchLabel();
                }
                else
                {
                    // If no match found at all, reset search index and match index
                    currentSearchIndex = 0;
                    currentMatchIndex = 0;
                    index = textValues.Text.IndexOf(word, StringComparison.OrdinalIgnoreCase);

                    if (index >= 0)
                    {
                        textValues.Select(index, word.Length);
                        textValues.SelectionBackColor = Color.Yellow;
                        textValues.ScrollToCaret();
                        currentSearchIndex = index + word.Length;
                        currentMatchIndex = 1; // Set current match index to 1
                        UpdateMatchLabel();
                    }
                }
            }
        }

        private void HighlightLastMatch(string word)
        {
            // Clear previous selection and reset background color
            textValues.SelectionStart = 0;
            textValues.SelectionLength = textValues.Text.Length;
            textValues.SelectionBackColor = textValues.BackColor;

            int index = textValues.Text.LastIndexOf(word, currentSearchIndex - 1, currentSearchIndex, StringComparison.OrdinalIgnoreCase);

            if (index >= 0)
            {
                textValues.Select(index, word.Length);
                textValues.SelectionBackColor = Color.Yellow;
                textValues.ScrollToCaret();
                currentSearchIndex = index;
                currentMatchIndex--;
                UpdateMatchLabel();
            }
            else
            {
                // If no more matches found, scroll to and highlight the last occurrence
                index = textValues.Text.LastIndexOf(word, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    textValues.Select(index, word.Length);
                    textValues.SelectionBackColor = Color.Yellow;
                    textValues.ScrollToCaret();
                    currentSearchIndex = index;
                    currentMatchIndex = totalMatches;
                    UpdateMatchLabel();
                }
            }
        }

        //Update Match Count
        private void UpdateMatchLabel()
        {
            labelSearch.Text = $"{currentMatchIndex} of {totalMatches}";
        }

        //User clicked next search term
        private void btnNext_Click(object sender, EventArgs e)
        {
            string wordToHighlight = textSearch.Text;
            HighlightNextMatch(wordToHighlight);
        }

        //user clicked previous search term
        private void btnPrev_Click(object sender, EventArgs e)
        {
            string wordToHighlight = textSearch.Text;
            HighlightLastMatch(wordToHighlight);
        }

        //User has updated the search term
        private void textSearch_TextChanged(object sender, EventArgs e)
        {
            string wordToHighlight = textSearch.Text;
            string richTextBoxText = textValues.Text.ToLower();

            wordToHighlight = wordToHighlight.ToLower();
            HighlightWord(wordToHighlight);
            totalMatches = richTextBoxText.Split(new[] { wordToHighlight }, StringSplitOptions.None).Length - 1;

            if (totalMatches > 0)
                currentMatchIndex = 1;
            else
                currentMatchIndex = 0;

            UpdateMatchLabel();
        }

        //Clear text preview from search box
        private void textSearch_Enter(object sender, EventArgs e)
        {
            textSearch.Text = "";
            textSearch.Font = new Font(textSearch.Font.FontFamily, 9, FontStyle.Regular);
            textSearch.ForeColor = Color.Black;

        }

        #endregion

    }
}