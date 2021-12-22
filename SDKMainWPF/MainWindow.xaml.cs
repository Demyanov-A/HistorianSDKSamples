// ------------------------------------------------------------------------------------------------------------
// <copyright company="Invensys Systems Inc" file="MainWindow.xaml.cs">
//   Copyright (C) 2013 Invensys Systems Inc.  All rights reserved.
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
// </copyright>
// <summary>
//
// </summary>
// ------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace SDKMainWPF
{
    /// <summary>
    /// This is a sample UI to interface with sample toolkit libraries
    /// </summary>
    public partial class MainWindow : Window
    {

        #region --Variables--
        private ConnectionBuilder.ConnectionBuilder connectionBuilder;
        private TagBuilder.TagBuilder tagBuilder;
        private StorageBuilder.StorageBuilder storageBuilder;
        private RetrievalBuilder.RetrievalBuilder retrievalBuilder;
        #endregion

        /// <summary>
        /// Constructor for MainWindow
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            //Set default server name
            textBoxServerName.Text = System.Environment.MachineName;

            //Populate combo box with values from enumerator for Historian datatype
            foreach (ArchestrA.HistorianDataType dt in Enum.GetValues(typeof(ArchestrA.HistorianDataType)))
            {
                comboBoxDataType.Items.Add(dt.ToString());
            }

            //Populate combo box with values from enumerator for Historian storagetype
            foreach (ArchestrA.HistorianStorageType st in Enum.GetValues(typeof(ArchestrA.HistorianStorageType)))
            {
                if (st.ToString() != "All")
                    comboBoxStorageType.Items.Add(st.ToString());
            }

            //Populate combo box with values from enumerator for Historian retrieval modes
            foreach (ArchestrA.HistorianRetrievalMode rm in Enum.GetValues(typeof(ArchestrA.HistorianRetrievalMode)))
            {
                comboBoxRetrievalModes.Items.Add(rm.ToString());
            }

            //Select first retrieval mode in list
            if (comboBoxRetrievalModes.Items.Count > 0)
                comboBoxRetrievalModes.SelectedIndex = 0;


            DateTime temp = DateTime.Now;
            SetDateTimePickers(temp.Subtract(new TimeSpan(0, Convert.ToInt32(comboBoxDuration.Text), 0)), temp);
        }

        /// <summary>
        /// Browse for store forward directory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBoxSFPath.Text = dialog.SelectedPath;
            }
        }

        /// <summary>
        /// This will initiate an asynchronous
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            if (connectionBuilder != null)
                connectionBuilder.Disconnect();
            //Reset status bar and disable connect button
            rectangleConnectedToServer.Stroke = new SolidColorBrush(Colors.Red);
            rectangleConnectedToStoreForward.Stroke = new SolidColorBrush(Colors.Red);
            rectangleConnectedToStorage.Stroke = new SolidColorBrush(Colors.Red);            
            textBlockError.Text = "";
            
            //Create instnace of connectionbuilder using connection arguments
            connectionBuilder = new ConnectionBuilder.ConnectionBuilder();
            connectionBuilder.UpdateStatus += new ConnectionBuilder.ConnectionBuilder.ConnectionStatusChanged(connectionBuilder_UpdateStatus);
            connectionBuilder.ConnectAsync(Convert.ToUInt32(textBoxMinSFDuration.Text),
                                    passwordBox.Password,
                                    false,
                                    textBoxServerName.Text,
                                    Convert.ToUInt32(textBoxFreeDiskSpace.Text),
                                    textBoxSFPath.Text,
                                    (ushort)Convert.ToInt32(textBoxTCPPort.Text),
                                    textBoxUserName.Text);

            tagBuilder = new TagBuilder.TagBuilder();
            storageBuilder = new StorageBuilder.StorageBuilder();
            retrievalBuilder = new RetrievalBuilder.RetrievalBuilder();
            retrievalBuilder.AnalogQueryCompleted += new RetrievalBuilder.RetrievalBuilder.AnalogQueryComplete(retrievalBuilder_AnalogQueryCompleted);
            retrievalBuilder.StateQueryCompleted += new RetrievalBuilder.RetrievalBuilder.StateQueryComplete(retrievalBuilder_StateQueryCompleted);
            retrievalBuilder.HistoryQueryCompleted += new RetrievalBuilder.RetrievalBuilder.HistoryQueryComplete(retrievalBuilder_HistoryQueryCompleted);
            listBoxTags.Items.Clear();
            
        }

        void retrievalBuilder_HistoryQueryCompleted(System.Collections.ObjectModel.ObservableCollection<ArchestrA.HistoryQueryResult> HistoryCollection)
        {
            try
            {
                Action action = () => dataGridResults.ItemsSource = HistoryCollection;
                Dispatcher.Invoke(action); 
            }
            catch { }
        }

        void retrievalBuilder_StateQueryCompleted(System.Collections.ObjectModel.ObservableCollection<ArchestrA.StateSummaryQueryResult> StateHistoryCollection)
        {
            try
            {
                Action action = () => dataGridResults.ItemsSource = StateHistoryCollection;
                Dispatcher.Invoke(action);                
            }
            catch { }
        }

        void retrievalBuilder_AnalogQueryCompleted(System.Collections.ObjectModel.ObservableCollection<ArchestrA.AnalogSummaryQueryResult> AnalogHistoryCollection)
        {
            try
            {
                Action action = () => dataGridResults.ItemsSource = AnalogHistoryCollection;
                Dispatcher.Invoke(action);                
            }
            catch { }

        }

        void connectionBuilder_UpdateStatus(ArchestrA.HistorianConnectionStatus status)
        {       
            Action action = () => rectangleConnectedToServer.Stroke = status!=null && status.ConnectedToServer ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
            Dispatcher.Invoke(action);
            action = () => rectangleConnectedToStoreForward.Stroke = status != null && status.ConnectedToStoreForward ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
            Dispatcher.Invoke(action);
            action = () => rectangleConnectedToStorage.Stroke = status != null && status.ConnectedToServerStorage ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
            Dispatcher.Invoke(action);

            if (status != null && status.ErrorOccurred)
                action = () => textBlockError.Text = status.Error.ErrorDescription;
            else
                action = () => textBlockError.Text = "";
            Dispatcher.Invoke(action);

        }

        private void buttonAddTags_Click(object sender, RoutedEventArgs e)
        {
            if (tagBuilder == null)
            {
                MessageBox.Show("Please connect first");
            }
            else
            {
                if (comboBoxStorageType.SelectedIndex == -1 || comboBoxDataType.SelectedIndex == -1)
                {
                    MessageBox.Show("You must select a storage and data type first");
                }
                else
                {
                    buttonAddTags.Content = "Please wait";
                    buttonAddTags.IsEnabled = false;
                    buttonAddTags.UpdateLayout();
                    textBoxTagsAdded.Text = "0";

                    if (!tagBuilder.AddTags(connectionBuilder.HistorianAccess,
                        textBoxPrefix.Text,
                        textBoxDescription.Text,
                        Convert.ToUInt32(textBoxNumberOfTags.Text),
                        checkBoxEnableChannelStatus.IsChecked.Value,
                        (ArchestrA.HistorianDataType)Enum.Parse(typeof(ArchestrA.HistorianDataType), comboBoxDataType.Text),
                        (ArchestrA.HistorianStorageType)Enum.Parse(typeof(ArchestrA.HistorianStorageType), comboBoxStorageType.Text)))
                    {
                        MessageBox.Show(tagBuilder.LastError.ErrorDescription);
                    }

                    textBoxTagsAdded.Text = tagBuilder.TagsAdded.Count.ToString();
                    listBoxTags.Items.Clear();                    
                    foreach (KeyValuePair<String, uint> pair in tagBuilder.TagsAdded)
                    {
                        listBoxTags.Items.Add(pair.Key);

                        if (!listBoxTagsToRetrieve.Items.Contains(pair.Key))
                        {
                            listBoxTagsToRetrieve.Items.Add(pair.Key);
                        }
                    }

                    buttonAddTags.IsEnabled = true;
                    buttonAddTags.Content = "Add Tags";
                    buttonAddTags.UpdateLayout();
                }
            }
        }

        private void buttonUnCheckAll_Click(object sender, RoutedEventArgs e)
        {
            listBoxTags.UnselectAll();
        }

        private void buttonCheckAll_Click(object sender, RoutedEventArgs e)
        {
            listBoxTags.SelectAll();
        }

        private void buttonStore_Click(object sender, RoutedEventArgs e)
        {
            if (storageBuilder == null)
            {
                MessageBox.Show("Please connect first");
            }
            else
            {
                if (buttonStore.Content.ToString() == "Stop")
                {
                    buttonStore.Content = "Store Values";
                    buttonStore.UpdateLayout();
                    storageBuilder.StopStoring();
                }
                else
                {
                    if (listBoxTags.SelectedItems.Count == 0)
                    {
                        MessageBox.Show("You have not selected any tags to store");
                    }
                    else
                    {
                        buttonStore.Content = "Stop";
                        buttonStore.UpdateLayout();
                        int threadct = Convert.ToInt32(textBoxSleepNumberofThreads.Text);
                        int numberoftagsperthread = listBoxTags.SelectedItems.Count / threadct;
                        int count = 0;
                        String[] tags = new String[numberoftagsperthread];
                        for (int i = 0; i < listBoxTags.SelectedItems.Count; i++) // (object tag in clbTags.CheckedItems)
                        {
                            tags[count] = listBoxTags.SelectedItems[i].ToString();
                            count++;
                            if (count == numberoftagsperthread)
                            {
                                storageBuilder.StoreValues(connectionBuilder.HistorianAccess, tags, Convert.ToInt32(textBoxValuesPerTag.Text), Convert.ToInt32(textBoxSleepTimeBetweenValues.Text), Convert.ToInt32(textBoxSleepTimeBetweenBatches.Text), !checkBoxStoreNonStreamed.IsChecked.Value);
                                tags = new String[numberoftagsperthread];
                                count = 0;
                            }
                        }

                        if (count > 0)
                        {
                            storageBuilder.StoreValues(connectionBuilder.HistorianAccess, tags, Convert.ToInt32(textBoxValuesPerTag.Text), Convert.ToInt32(textBoxSleepTimeBetweenValues.Text), Convert.ToInt32(textBoxSleepTimeBetweenBatches.Text), !checkBoxStoreNonStreamed.IsChecked.Value);
                        }
                    }
                }
            }
        }

        private void buttonExecuteRetrieval_Click(object sender, RoutedEventArgs e)
        {
            if (retrievalBuilder == null)
            {
                MessageBox.Show("Please connect first");
            }
            else
            {
                if (radioButtonAnalogSummary.IsChecked.Value)
                {
                    ArchestrA.AnalogSummaryQueryArgs queryArgs = new ArchestrA.AnalogSummaryQueryArgs();
                    queryArgs.TagNames = new System.Collections.Specialized.StringCollection();
                    foreach (String tag in listBoxTagsToRetrieve.Items)
                        queryArgs.TagNames.Add(tag);
                    queryArgs.StartDateTime = dateTimePickerStart.Value.Value;
                    queryArgs.EndDateTime = dateTimePickerEnd.Value.Value;
                    queryArgs.RetrievalMode = (ArchestrA.HistorianRetrievalMode)Enum.Parse(typeof(ArchestrA.HistorianRetrievalMode), comboBoxRetrievalModes.Text);
                    queryArgs.Resolution = (ulong)Convert.ToUInt32(textBoxResolution.Text);

                    if (checkBoxAutoRefresh.IsChecked.Value)
                    {
                        retrievalBuilder.RetrieveAnalogSummaryValues(connectionBuilder.HistorianAccess, queryArgs, true, (ulong)Convert.ToUInt32(comboBoxDuration.Text) * 60000);
                    }
                    else
                    {
                        dataGridResults.ItemsSource = retrievalBuilder.RetrieveAnalogSummaryValues(connectionBuilder.HistorianAccess, queryArgs);
                    }
                }
                else if (radioButtonHistory.IsChecked.Value)
                {
                    ArchestrA.HistoryQueryArgs queryArgs = new ArchestrA.HistoryQueryArgs();
                    queryArgs.TagNames = new System.Collections.Specialized.StringCollection();
                    foreach (String tag in listBoxTagsToRetrieve.Items)
                        queryArgs.TagNames.Add(tag);
                    queryArgs.StartDateTime = dateTimePickerStart.Value.Value;
                    queryArgs.EndDateTime = dateTimePickerEnd.Value.Value;
                    queryArgs.Resolution = (ulong)Convert.ToUInt32(textBoxResolution.Text);
                    queryArgs.TimeDeadband = Convert.ToUInt32(textBoxTimeDeadband.Text);
                    queryArgs.ValueDeadband = Convert.ToUInt32(textBoxValueDeadband.Text);
                    queryArgs.RetrievalMode = (ArchestrA.HistorianRetrievalMode)Enum.Parse(typeof(ArchestrA.HistorianRetrievalMode), comboBoxRetrievalModes.Text);

                    if (checkBoxAutoRefresh.IsChecked.Value)
                    {
                        retrievalBuilder.RetrieveHistoryValues(connectionBuilder.HistorianAccess, queryArgs, true, (ulong)Convert.ToUInt32(comboBoxDuration.Text) * 60000);
                    }
                    else
                    {
                        dataGridResults.ItemsSource = retrievalBuilder.RetrieveHistoryValues(connectionBuilder.HistorianAccess, queryArgs);
                    }

                }
                else if (radioButtonStateSummary.IsChecked.Value)
                {
                    ArchestrA.StateSummaryQueryArgs queryArgs = new ArchestrA.StateSummaryQueryArgs();
                    queryArgs.TagNames = new System.Collections.Specialized.StringCollection();
                    foreach (String tag in listBoxTagsToRetrieve.Items)
                        queryArgs.TagNames.Add(tag);
                    queryArgs.StartDateTime = dateTimePickerStart.Value.Value;
                    queryArgs.EndDateTime = dateTimePickerEnd.Value.Value;
                    queryArgs.RetrievalMode = (ArchestrA.HistorianRetrievalMode)Enum.Parse(typeof(ArchestrA.HistorianRetrievalMode), comboBoxRetrievalModes.Text);
                    queryArgs.Resolution = (ulong)Convert.ToUInt32(textBoxResolution.Text);                    

                    if (checkBoxAutoRefresh.IsChecked.Value)
                    {
                        retrievalBuilder.RetrieveStateSummaryValues(connectionBuilder.HistorianAccess, queryArgs, true, (ulong)Convert.ToUInt32(comboBoxDuration.Text) * 60000);
                    }
                    else
                    {
                        dataGridResults.ItemsSource = retrievalBuilder.RetrieveStateSummaryValues(connectionBuilder.HistorianAccess, queryArgs);
                    }

                }
            }
        }

        private void buttonNewTag_Click(object sender, RoutedEventArgs e)
        {
            if (retrievalBuilder == null)
            {
                MessageBox.Show("Please connect first");
            }
            else
            {
                ArchestrA.HistorianTag newtag;
                ArchestrA.HistorianAccessError error;
                if (connectionBuilder.HistorianAccess.GetTagInfoByName(textBoxNewTag.Text, true, out newtag, out error))
                {
                    listBoxTagsToRetrieve.Items.Add(newtag.TagName);
                }
                else
                {
                    MessageBox.Show(String.Format
                    ("Failed to find tag: {0}", error.ErrorDescription));
                }
            }
        }

        private void buttonRemoveTag_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxTagsToRetrieve.SelectedIndex >= 0)
            {
                listBoxTagsToRetrieve.Items.RemoveAt(listBoxTagsToRetrieve.SelectedIndex);
            }
            else
            {
                MessageBox.Show("You must first select a tag to remove");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (connectionBuilder != null)
                connectionBuilder.Disconnect();    
        }

        private void comboBoxDuration_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count>0)
            {
                DateTime temp = DateTime.Now;
                SetDateTimePickers(temp.Subtract(new TimeSpan(0, Convert.ToInt32(((ComboBoxItem)e.AddedItems[0]).Content.ToString()), 0)), temp);
            }
        }

        private void SetDateTimePickers(DateTime StartTime, DateTime Endtime)
        {
            dateTimePickerStart.Value = StartTime;            
            dateTimePickerEnd.Value = Endtime;            
        }

        private void buttonStopRetrieval_Click(object sender, RoutedEventArgs e)
        {
            if (retrievalBuilder != null)
                retrievalBuilder.StopAllThreads();
        }

    }
}
