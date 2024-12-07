﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FacebookWrapper.ObjectModel;

namespace BasicFacebookFeatures
{

    internal class ActivityCenter
    {

        private readonly List<Post> r_UserPosts;
        private readonly List<Photo> r_UserPhotos;

        private readonly Dictionary<int, int> r_YearCounts = new Dictionary<int, int>();
        private readonly Dictionary<int, int> r_MonthCounts = new Dictionary<int, int>();
        private readonly Dictionary<int, int> r_HourCounts = new Dictionary<int, int>();

        public List<Post> FilteredPosts { get; private set; }
        public List<Photo> FilteredPhotos { get; private set; }


        public ActivityCenter(AppManager i_AppManager)
        {
            r_UserPhotos = i_AppManager.UserPhotos;
            r_UserPosts = i_AppManager.UserPosts;

            initializeCounts(r_YearCounts, 2010, DateTime.Now.Year);
            initializeCounts(r_MonthCounts, 1, 12);
            initializeCounts(r_HourCounts, 0, 23);

            FilteredPosts = new List<Post>();
            FilteredPhotos = new List<Photo>();

            processPosts();
            processPhotos();
        }

        private void initializeCounts(Dictionary<int, int> i_CountDict, int i_From, int i_To)
        {
            for(int i = i_From; i <= i_To; i++)
            {
                i_CountDict[i] = 0;
            }
        }

        private void processPosts()
        {
            foreach(Post post in r_UserPosts)
            {
                if(post.CreatedTime.HasValue)
                {
                    processTimeData(post.CreatedTime.Value);
                }
            }
        }

        private void processPhotos()
        {
            foreach(Photo photo in r_UserPhotos)
            {
                if(photo.CreatedTime.HasValue)
                {
                    processTimeData(photo.CreatedTime.Value);
                }
            }
        }

        private void processTimeData(DateTime? i_CreatedTime)
        {
            if(i_CreatedTime.HasValue)
                if(i_CreatedTime.HasValue)
                {
                    DateTime createdTime = i_CreatedTime.Value;

                    if(r_YearCounts.ContainsKey(createdTime.Year))
                    {
                        r_YearCounts[createdTime.Year]++;
                    }

                    if(r_MonthCounts.ContainsKey(createdTime.Month))
                    {
                        r_MonthCounts[createdTime.Month]++;
                    }

                    if(r_HourCounts.ContainsKey(createdTime.Hour))
                    {
                        r_HourCounts[createdTime.Hour]++;
                    }
                }
        }

        public List<KeyValuePair<int, int>> GetYearCounts()
        {
            return r_YearCounts.OrderByDescending(pair => pair.Value).ToList();
        }

        public List<KeyValuePair<int, int>> GetMonthCounts(int i_Year)
        {
            Dictionary<int, int> monthCounts = new Dictionary<int, int>();
            foreach(Post post in r_UserPosts)
            {
                if(post.CreatedTime.HasValue && post.CreatedTime.Value.Year == i_Year)
                {
                    int month = post.CreatedTime.Value.Month;

                    if(!monthCounts.ContainsKey(month))
                    {
                        monthCounts[month] = 0;
                    }

                    monthCounts[month]++;
                }
            }

            return monthCounts.OrderByDescending(pair => pair.Value).ToList();
        }


        public List<KeyValuePair<int, int>> GetHourCounts(int i_Year, int i_Month)
        {
            Dictionary<int, int> hourCounts = new Dictionary<int, int>();

            foreach(Post post in r_UserPosts)
            {
                if(post.CreatedTime.HasValue)
                {
                    DateTime createdTime = post.CreatedTime.Value;
                    if(createdTime.Year == i_Year && createdTime.Month == i_Month)
                    {
                        if(!hourCounts.ContainsKey(createdTime.Hour))
                        {
                            hourCounts[createdTime.Hour] = 0;
                        }

                        hourCounts[createdTime.Hour]++;
                    }
                }
            }

            foreach(Photo photo in r_UserPhotos)
            {
                if(photo.CreatedTime.HasValue)
                {
                    DateTime createdTime = photo.CreatedTime.Value;
                    if(createdTime.Year == i_Year && createdTime.Month == i_Month)
                    {
                        if(!hourCounts.ContainsKey(createdTime.Hour))
                        {
                            hourCounts[createdTime.Hour] = 0;
                        }

                        hourCounts[createdTime.Hour]++;
                    }
                }
            }

            return hourCounts.OrderByDescending(pair => pair.Value).ToList();
        }


        public List<Post> GetPostsByTime(int? i_Year = null, int? i_Month = null, int? i_Hour = null)
        {
            List<Post> filteredPosts = new List<Post>();

            foreach(Post post in r_UserPosts)
            {
                if(post.CreatedTime.HasValue)
                {
                    DateTime createdTime = post.CreatedTime.Value;

                    if((!i_Year.HasValue || createdTime.Year == i_Year)
                       && (!i_Month.HasValue || createdTime.Month == i_Month)
                       && (!i_Hour.HasValue || createdTime.Hour == i_Hour))
                    {
                        filteredPosts.Add(post);
                    }

                }
            }

            return filteredPosts;
        }

        public List<Photo> GetPhotosByTime(int? i_Year = null, int? i_Month = null, int? i_Hour = null)
        {
            List<Photo> filteredPhotos = new List<Photo>();

            foreach(Photo photo in r_UserPhotos)
            {
                if(photo.CreatedTime.HasValue)
                {
                    DateTime createdTime = photo.CreatedTime.Value;

                    if((!i_Year.HasValue || createdTime.Year == i_Year)
                       && (!i_Month.HasValue || createdTime.Month == i_Month)
                       && (!i_Hour.HasValue || createdTime.Hour == i_Hour))
                    {
                        filteredPhotos.Add(photo);
                    }

                }
            }

            return filteredPhotos;
        }


    }

}