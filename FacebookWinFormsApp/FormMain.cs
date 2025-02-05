﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FacebookWrapper.ObjectModel;
using FacebookWrapper;
using System.Windows.Forms.VisualStyles;
using CefSharp.DevTools.Fetch;
using static System.Windows.Forms.AxHost;
using static BasicFacebookFeatures.ActivityCenter;
using Facebook;

namespace BasicFacebookFeatures
{
    public partial class FormMain : Form
    {
        private readonly AppManager r_AppManager;
        private Action onLogin;
        private Action onLogout;
        private FindFriends m_FindFriends;
        private ActivityCenter m_ActivityCenter;

        private readonly List<Panel> r_HomePanels;
        private readonly List<TabPage> r_AddedTabs;

        public FormMain()
        {
            InitializeComponent();
            r_AppManager = new AppManager();

            m_FindFriends = new FindFriends();
            FacebookWrapper.FacebookService.s_CollectionLimit = 25;
            r_HomePanels = new List<Panel>
                           {
                               panelAlbums,
                               panelStatusPost,
                               panelFavoriteTeams,
                               panelFriends,
                               panelLikes,
                               panelGroups
                           };
            r_AddedTabs = new List<TabPage> { tabMyProfile, tabActivityCenter, tabPage1 };

            updateTabs(false);

            onLogin += updateLoginButton;
            onLogin += updateHomePanelsVisible;
            onLogin += fetchProfileInfo;
            onLogin += fetchLikedPages;
            onLogin += fetchAlbums;
            onLogin += fetchFriendList;
            onLogin += fetchMyProfile;
            onLogin += fetchActivityCenter;
            onLogin += fetchFriendsLookupPage;
            onLogin += fetchGroups;
            onLogin += fetchFavoriteTeams;
            onLogin += fetchStatusPost;

            onLogout += updateHomePanelsVisible;
            onLogout += unLaunchFacebook;
            onLogout += updateLoginButton;
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            if (r_AppManager.LoginResult == null)
            {
                performLogin();
            }
        }
         
        private void performLogin()
        {
            try
            {
                r_AppManager.Login();
                updateTabs(r_AppManager.IsLoggedIn);
                onLogin?.Invoke();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonLogout_Click(object sender, EventArgs e)
        {
            performLogout();
        }

        private void performLogout()
        {
            try
            {
                r_AppManager.Logout();
                updateTabs(r_AppManager.IsLoggedIn);
                onLogout?.Invoke();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void updateHomePanelsVisible()
        {
            bool isVisibile;
            isVisibile = r_AppManager.IsLoggedIn;

            foreach(Panel panel in r_HomePanels)
            {
                panel.Visible = isVisibile;
            }
        }

        private void updateTabs(bool i_IsVisible = true)
        {
            tabsController.SelectedIndex = 0;
            if (i_IsVisible)
            {
                foreach (TabPage tab in r_AddedTabs)
                {
                    tabsController.TabPages.Add(tab);
                }
            }
            else
            {
                foreach (TabPage tab in r_AddedTabs)
                {
                    tabsController.TabPages.Remove(tab);
                }
            }
        }

        private void updateLoginButton()
        {
            if(r_AppManager.IsLoggedIn)
            {
                buttonLogin.Text = $"Logged in as {r_AppManager.LoggedInUser.Name}";
                buttonLogin.BackColor = Color.LightGreen;
                buttonLogin.Enabled = false;
                buttonLogout.Enabled = true;
            }
            else
            {
                buttonLogin.BackColor = buttonLogout.BackColor;
                buttonLogin.Enabled = true;
                buttonLogout.Enabled = false;
                buttonLogin.Text = "Login";
            }
        }

        private void fetchProfileInfo()
        {
            labelUserName.Visible = true;
            pictureBoxProfile.Visible = true;
            labelUserName.Text = $"Hello, {r_AppManager.LoggedInUser.FirstName}!";
            pictureBoxProfile.ImageLocation = r_AppManager.LoggedInUser.PictureNormalURL;
        }


        private void fetchFriendsLookupPage()
        {
            fetchFriendsComboBox();
            populateRealitionshipStatusList();
            populateLikedPagesList();
            populateGenderComboBox();

        }

        private void populateGenderComboBox()
        {
            comboBoxGender.Items.Clear();
            comboBoxGender.SelectedItem = null;
            comboBoxGender.SelectedIndex = -1;
            comboBoxGender.Text = "";
            foreach (User.eGender gender in Enum.GetValues(typeof(User.eGender)))
            {
                comboBoxGender.Items.Add(gender);
            }
            comboBoxGender.Items.Add("No Preference");
            comboBoxGender.SelectedIndex = 0;
        }

        private void populateRealitionshipStatusList()
        {
            checkedListBoxRealitionshipStatus.Visible = true;
            checkedListBoxRealitionshipStatus.Items.Clear();

            foreach (User.eRelationshipStatus relationshipStatus in Enum.GetValues(typeof(User.eRelationshipStatus)))
            {
                checkedListBoxRealitionshipStatus.Items.Add(relationshipStatus);
            }

        }

        private void populateLikedPagesList()
        {
            checkedListBoxlikedPages.Items.Clear();
            checkedListBoxlikedPages.DisplayMember = "Name";
            foreach (Page page in r_AppManager.LoggedInUser.LikedPages)
            {
                checkedListBoxlikedPages.Items.Add(page);
            }

            if (checkedListBoxlikedPages.Items.Count == 0)
            {
                checkedListBoxlikedPages.Items.Add("No liked pages to retrieve");
            }
        }


        private void fetchFriendsComboBox()
        {
            comboBoxFriendList.Items.Clear();
            comboBoxFriendList.SelectedItem = null;
            comboBoxFriendList.SelectedIndex = -1;
            comboBoxFriendList.Text = "";
            comboBoxFriendList.DisplayMember = "Name";

            foreach (User user in r_AppManager.LoggedInUser.Friends)
            {
                comboBoxFriendList.Items.Add(user);
            }

            comboBoxFriendList.SelectedIndex = 0;
        }


        private void fetchFriendList()
        {
            panelFriends.Visible = true;
            listBoxUserFriends.Items.Clear();
            pictureBoxUserFriend.Image = null;
            listBoxUserFriends.DisplayMember = "Name";
            foreach (User user in r_AppManager.LoggedInUser.Friends)
            {
                listBoxUserFriends.Items.Add(user);
            }

            if (listBoxUserFriends.Items.Count == 0)
            {
                listBoxUserFriends.Items.Add("No friends to retrieve");
            }
        }

        private void fetchMyProfile()
        {
            labelEmailData.Text = r_AppManager.LoggedInUser.Email;
            labelBirthdayData.Text = r_AppManager.LoggedInUser.Birthday;
            labelGenderData.Text = r_AppManager.LoggedInUser.Gender.ToString();
            labelFullNameData.Text = r_AppManager.LoggedInUser.Name;
            PictureBoxMyProfile.Image = r_AppManager.LoggedInUser.ImageLarge;
        }

        private void fetchActivityCenter()
        {
            m_ActivityCenter = new ActivityCenter(r_AppManager);

            listBoxFilteredPosts.Visible = true;
            listBoxFilteredPosts.Items.Clear();

            listBoxYear.Visible = true;
            listBoxMonth.Visible = true;
            listBoxHour.Visible = true;

            displayYearCounts();
            displayHoursCounts();
        }

        private void displayYearCounts(string i_SortBy = "CountDescending")
        {
            listBoxYear.Items.Clear();
            List<KeyValuePair<int, int>> yearCounts = m_ActivityCenter.GetYearCounts(i_SortBy);
            foreach (KeyValuePair<int, int> year in yearCounts)
            {
                listBoxYear.Items.Add($"{year.Key}: {year.Value} posts/photos");
            }

            if(listBoxYear.Items.Count > 0)
            {
                populateSortComboBox(comboBoxYearSort, "Year");
            }
        }

        private void displayMonthCounts(int i_SelectedYear, string i_SortBy = "CountDescending")
        {
            listBoxMonth.Items.Clear();
            List<KeyValuePair<int, int>> monthCounts = m_ActivityCenter.GetMonthCounts(i_SelectedYear, i_SortBy);
            listBoxMonth.Items.Clear();

            foreach (KeyValuePair<int, int> month in monthCounts)
            {
                listBoxMonth.Items.Add($"{UserUtils.sr_Months[month.Key - 1]}: {month.Value} posts/photos");
            }

            if(listBoxMonth.Items.Count > 0)
            {
                populateSortComboBox(comboBoxMonthSort, "Month");
            }
        }

        private void displayHoursCounts(string i_SortBy = "CountDescending")
        {
            listBoxHour.Items.Clear();
            List<KeyValuePair<int, int>> hoursCounts = m_ActivityCenter.GetHourCounts(i_SortBy);
            foreach (KeyValuePair<int, int> hour in hoursCounts)
            {
                listBoxHour.Items.Add($"{makeHourFormat(hour.Key)}: {hour.Value} posts/photos");
            }

            if(listBoxHour.Items.Count > 0)
            {
                populateSortComboBox(comboBoxHourSort, "Hour");
            }
        }

        private void populateSortComboBox(ComboBox i_ComboBox, string i_TimeType)
        {
            i_ComboBox.Items.Clear();
            i_ComboBox.Items.Add($"Sort by {i_TimeType} Ascending");
            i_ComboBox.Items.Add($"Sort by {i_TimeType} Descending");
            i_ComboBox.Items.Add("Sort by Count Ascending");
            i_ComboBox.Items.Add("Sort by Count Descending");
        }

        private string getSorting(int i_ComboBoxIndex)
        {
            string sorting;
            switch (i_ComboBoxIndex)
            {
                case 0:
                    sorting = "TimeAscending";
                    break;
                case 1:
                    sorting = "TimeDescending";
                    break;
                case 2:
                    sorting = "CountAscending";
                    break;
                case 3:
                    sorting = "CountDescending";
                    break;
                default:
                    sorting = "CountDescending";
                    break;
            }

            return sorting;
        }


        private void comboBoxYearSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBoxMonth.Items.Clear();
            comboBoxMonthSort.Items.Clear();
            string sorting = getSorting(comboBoxYearSort.SelectedIndex);
            displayYearCounts(sorting);
        }

        private void comboBoxMonthSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedYear = int.Parse(listBoxYear.SelectedItem.ToString().Split(':')[0]);
            string sorting = getSorting(comboBoxMonthSort.SelectedIndex);
            displayMonthCounts(selectedYear, sorting);
        }

        private void comboBoxHourSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            string sorting = getSorting(comboBoxHourSort.SelectedIndex);
            displayHoursCounts(sorting);
        }


        private void listBoxYear_SelectedIndexChanged(object sender, EventArgs e)
        {
           if(listBoxYear.SelectedItem != null)
           {
               int selectedYear = int.Parse(listBoxYear.SelectedItem.ToString().Split(':')[0]);
               labelDateOfPosts.Text = $"Your posts from {selectedYear}";
               filterPostsByTime(i_Year: selectedYear);

               displayMonthCounts(selectedYear);
           }
        }


        private void listBoxMonth_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxYear.SelectedItem != null && listBoxMonth.SelectedItem != null)
            {
                int selectedYear = int.Parse(listBoxYear.SelectedItem.ToString().Split(':')[0]);
                string selectedMonthName = listBoxMonth.SelectedItem.ToString().Split(':')[0];
                int selectedMonth = Array.IndexOf(UserUtils.sr_Months, selectedMonthName) + 1; ;

                labelDateOfPosts.Text = $"Your posts from {selectedMonthName} {selectedYear}";
                filterPostsByTime(i_Year: selectedYear, i_Month: selectedMonth);
            }
        }

        private void listBoxHour_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(listBoxHour.SelectedItem != null)
            {
                int selectedHour = int.Parse(listBoxHour.SelectedItem.ToString().Split(':')[0]);
                labelDateOfPosts.Text = $"Your posts from {makeHourFormat(selectedHour)} in total: ";

                filterPostsByTime(i_Hour: selectedHour);
            }
        }

        private string makeHourFormat(int i_Hour)
        {
            return $"{i_Hour.ToString("D2")}:00";
        }

        private void filterPostsByTime(int? i_Year = null, int? i_Month = null, int? i_Hour = null)
        {
            List<Post> posts = m_ActivityCenter.GetPostsByTime(i_Year, i_Month, i_Hour);
            List<Photo> photos = m_ActivityCenter.GetPhotosByTime(i_Year, i_Month, i_Hour);

            listBoxFilteredPosts.Items.Clear();
            addPostsToListbox(posts, listBoxFilteredPosts);
            addPhotosToListbox(photos, listBoxFilteredPosts);

            //albumFilteredPhotos.SetPhotos(photos);
        }


        private void addPostsToListbox(List<Post> i_Posts, ListBox i_ListBox)
        {
            foreach (Post post in i_Posts)
            {
                if (post.Message == null)
                {
                    i_ListBox.Items.Add("[No message]");
                }
                else
                {
                    i_ListBox.Items.Add(post.Message);
                }
            }
        }

        private void addPhotosToListbox(List<Photo> i_Photos, ListBox i_ListBox)
        {
            foreach (Photo photo in i_Photos)
            {
                if (photo.Name == null)
                {
                    i_ListBox.Items.Add("[No title]");
                }
                else
                {
                    i_ListBox.Items.Add(photo.Message);
                }
            }
        }

        private void fetchStatusPost()
        {
            panelStatusPost.Visible = true;
            textBoxStatusPost.Click += textBoxStatus_Click;
            textBoxStatusPost.Leave += textBoxStatus_Leave;
            if (r_AppManager.LoggedInUser != null)
            {
                textBoxStatusPost.Text = $"What's on your mind, {r_AppManager.LoggedInUser.FirstName}";
                textBoxStatusPost.ForeColor = Color.Gray;
            }
        }

        private void textBoxStatus_Click(object sender, EventArgs e)
        {
            if ( r_AppManager.LoggedInUser != null)
            {
                textBoxStatusPost.Text = ""; 
                textBoxStatusPost.ForeColor = Color.Black;
            }
        }

        private void textBoxStatus_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxStatusPost.Text))
            {
                textBoxStatusPost.Text = $"What's on your mind, {r_AppManager.LoggedInUser.Name}?";
                textBoxStatusPost.ForeColor = Color.Gray; 
            }
        }


        private void fetchAlbums()
        {
            panelAlbums.Visible = true;
            listBoxUserAlbums.Items.Clear();
            //albumUserAlbums.ClearPictureBoxInAlbum();
            listBoxUserAlbums.DisplayMember = "Name";
            foreach (FacebookWrapper.ObjectModel.Album album in r_AppManager.LoggedInUser.Albums)
            {
                listBoxUserAlbums.Items.Add(album);
            }

            if (listBoxUserAlbums.Items.Count == 0)
            {
                listBoxUserAlbums.Items.Add("No Albums to retrieve");
            }
        }
        private void fetchFavoriteTeams()
        {
            panelFavoriteTeams.Visible = true;
            pictureBoxFavoriteTeam.Image = null;
            listBoxUserFavoriteTeams.Items.Clear();
            listBoxUserFavoriteTeams.DisplayMember = "Name";
            foreach (Page page in r_AppManager.LoggedInUser.FavofriteTeams)
            {
                listBoxUserFavoriteTeams.Items.Add(page);
            }

            if (listBoxUserFavoriteTeams.Items.Count == 0)
            {
                listBoxUserFavoriteTeams.Items.Add("No Albums to retrieve");
            }
        }
        private void fetchGroups()
        {
            panelGroups.Visible = true;
            listBoxUserGroups.Items.Clear();
            listBoxUserGroups.DisplayMember = "Name";
            foreach (Group group in r_AppManager.LoggedInUser.Groups)
            {
                listBoxUserGroups.Items.Add(group);
            }

            if (listBoxUserGroups.Items.Count == 0)
            {
                listBoxUserGroups.Items.Add("No Groups to retrieve");
            }
        }

        private void unLaunchFacebook()
        {
            labelUserName.Visible = false;
            pictureBoxProfile.Visible = false;
        }

        private void fetchLikedPages()
        {
            panelLikes.Visible = true;
            listBoxLikes.Items.Clear();
            listBoxLikes.DisplayMember = "Name";

            try
            {
                if (r_AppManager.LoggedInUser.LikedPages != null && r_AppManager.LoggedInUser.LikedPages.Count > 0)
                {
                    foreach (Page likedPage in r_AppManager.LoggedInUser.LikedPages)
                    {
                        listBoxLikes.Items.Add(likedPage);
                    }
                }
                else
                {
                    listBoxLikes.Items.Add("No liked pages to display.");
                }
            }
            catch (Exception ex)
            {
                listBoxLikes.Items.Add("Couldn't fetch liked pages.");
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void userFavoriteTeamsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxUserFavoriteTeams.SelectedItems.Count == 1)
            {
                Page selectedTeam = listBoxUserFavoriteTeams.SelectedItem as Page;
                pictureBoxFavoriteTeam.LoadAsync(selectedTeam.PictureNormalURL);
            }
        }

        private void userFriendsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxUserFriends.SelectedItems.Count == 1)
            {
                User selectedUser = listBoxUserFriends.SelectedItem as User;
                pictureBoxUserFriend.LoadAsync(selectedUser.PictureNormalURL);
            }

        }

        private void userAlbumsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxUserAlbums.SelectedItems.Count == 1)
            {
                FacebookWrapper.ObjectModel.Album selectedAlbum = listBoxUserAlbums.SelectedItem as FacebookWrapper.ObjectModel.Album;

                if (selectedAlbum != null)
                {
                    //albumUserAlbums.SetPhotos(selectedAlbum.Photos.ToList());
                }
            }

        }

        private void updateFilteredFriendsListBox(HashSet<User> i_FilterdFriends)
        {
            listBoxFilteredUsers.Items.Clear();
            listBoxUserFavoriteTeams.DisplayMember = "Name";
            foreach(User user in i_FilterdFriends)
            {
                listBoxFilteredUsers.Items.Add(user);
            }

            if(listBoxFilteredUsers.Items.Count == 0)
            {
                listBoxFilteredUsers.Items.Add("No Users to retrieve");
            }
        }


        private HashSet<User.eRelationshipStatus> getUserSelectedRelationshipStatuses()
        {
            HashSet<User.eRelationshipStatus> selectedStatuses = new HashSet<User.eRelationshipStatus>();

            foreach (User.eRelationshipStatus item in checkedListBoxRealitionshipStatus.CheckedItems)
            {
                if (item is User.eRelationshipStatus status)
                {
                    selectedStatuses.Add(status);
                }
            }

            return selectedStatuses;
        }

        private HashSet<String> getUserSelectedLikedPagesId()
        {
            HashSet<String> selectedLikedPages = new HashSet<String>();

            foreach (Page item in checkedListBoxlikedPages.CheckedItems)
            {
                if (item is Page page)
                {
                    selectedLikedPages.Add(page.Id);
                }
            }

            return selectedLikedPages;
        }



        private void buttonApplySearch_Click(object sender, EventArgs e)
        {
            List<IFilterable> filterable = new List<IFilterable>();
            if (comboBoxFriendList.SelectedItem == null)
            {
                return;
            }
            User selectedFriend = comboBoxFriendList.SelectedItem as User;
            int minAge = (int)numericUpDownMinimumAge.Value;
            int maxAge = (int)numericUpDownMaximumAge.Value;
            filterable.Add(new FilterAge(minAge, maxAge));
            if (comboBoxGender.SelectedItem != null && !comboBoxGender.SelectedItem.Equals("No Preference"))
            {

                filterable.Add(new FilterGender((User.eGender)comboBoxGender.SelectedItem));
            }
            filterable.Add(new FilterRelationshipStatus(getUserSelectedRelationshipStatuses()));
            filterable.Add(new FilterLikedPages(getUserSelectedLikedPagesId()));

            HashSet<User> filterdFriends = m_FindFriends.GetFriendUserCommonFriendsPages(filterable, selectedFriend);
            updateFilteredFriendsListBox(filterdFriends);
        }

        private void numericUpDownMaximumAge_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDownMinimumAge.Value < 18)
            {
                numericUpDownMinimumAge.Value = 18;
            }

            if (numericUpDownMaximumAge.Value < numericUpDownMinimumAge.Value)
            {
                numericUpDownMaximumAge.Value = numericUpDownMinimumAge.Value;
            }

        }

        private void numericUpDownMinimumAge_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDownMaximumAge.Value < 18)
            {
                numericUpDownMaximumAge.Value = 18;
            }

            if (numericUpDownMaximumAge.Value < numericUpDownMinimumAge.Value)
            {
                numericUpDownMaximumAge.Value = numericUpDownMinimumAge.Value;
            }

        }

        private void listBoxFilteredUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBoxFilteredUsers.Image = (listBoxFilteredUsers.SelectedItem as User).ImageNormal;
        }

        private void buttonSetStatusPost_Click(object sender, EventArgs e)
        {
            string postData = textBoxStatusPost.Text;
            try
            {
                 r_AppManager.LoggedInUser.PostStatus(postData);
                 MessageBox.Show("New status was Posted!", "New Status Posted");
           
            }
            catch (FacebookOAuthException)
            {
                MessageBox.Show("cannot upload status.", "Facebook server error");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
    
}
