using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using LinqToTwitter;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace WebApplication1
{
    [HubName("twitty")]
    public class TweetHub : Hub
    {
        public void SayMessage(string message)
        {
            Clients.All.speak(message);
            Clients.All.feed(message);
        }

        public void CallHandler(string message)
        {
            var hashtag = "angular";

            if (!string.IsNullOrEmpty(message))
            {
                hashtag = message.ToLower();
            }
            var authorizer = TweetUserAuthorizer();

            var twitterContext = new TwitterContext(authorizer);

            var hashTweets = (from search in twitterContext.Search
                where search.Type == SearchType.Search &&
                      search.Query == hashtag
                select search).FirstOrDefault(); //.ToList();

            var jsonData = JsonConvert.SerializeObject(new {hashTweets=hashTweets, count=UserHandler.ConnectedIds.Count, hashTag=hashtag });
            Clients.All.speak(jsonData);
        }

        public void FeedHandler(string message)
        {
            var twitterAccountToDisplay = string.Empty;
            var statusTweets = new List<Status>();
            if (!string.IsNullOrEmpty(message))
            {
                twitterAccountToDisplay = message.ToLower();

                var authorizer = TweetUserAuthorizer();

                var twitterContext = new TwitterContext(authorizer);

                 statusTweets = (from tweet in twitterContext.Status
                    where tweet.Type == StatusType.User &&
                          tweet.ScreenName == twitterAccountToDisplay &&
                          tweet.IncludeContributorDetails == true &&
                          tweet.IncludeEntities == true
                    select tweet).ToList();
            }
  

            var jsonData = JsonConvert.SerializeObject(new { statusTweets = statusTweets, count = UserHandler.ConnectedIds.Count, feedTag = twitterAccountToDisplay });
            Clients.All.feed(jsonData);
        }


        private static SingleUserAuthorizer TweetUserAuthorizer()
        {
            var authorizer = new SingleUserAuthorizer
            {
                CredentialStore = new InMemoryCredentialStore
                {
                    ConsumerKey = ConfigurationManager.AppSettings["consumerKey"],
                    ConsumerSecret = ConfigurationManager.AppSettings["consumerSecret"],
                    OAuthToken = ConfigurationManager.AppSettings["oauthToken"],
                    OAuthTokenSecret = ConfigurationManager.AppSettings["OAuthTokenSecret"]
                }
            };
            return authorizer;
        }

        public override Task OnConnected()
        {
            UserHandler.ConnectedIds.Add(Context.ConnectionId);
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            UserHandler.ConnectedIds.Remove(Context.ConnectionId);
           return base.OnDisconnected(stopCalled);
        }
    }

    public static class UserHandler
    {
        public static HashSet<string> ConnectedIds = new HashSet<string>();
    }


}