using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using LinqToTwitter;
using System.Configuration;

namespace WebApplication1.Controllers
{
    public class TwitterController : Controller
    {


        public ActionResult Index()
        {

            var twitterAccountToDisplay = "roeburg";
            var hashtag = "angular"; 

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

            var twitterContext = new TwitterContext(authorizer);

            var statusTweets = (from tweet in twitterContext.Status
                               where tweet.Type == StatusType.User &&
                                       tweet.ScreenName == twitterAccountToDisplay &&
                                       tweet.IncludeContributorDetails == true &&
                                       tweet.Count == 10 &&
                                       tweet.IncludeEntities == true
                               select tweet).ToList();


            var hashTweets = (from search in twitterContext.Search
                where search.Type == SearchType.Search &&
                      search.Query == hashtag 
                select search).FirstOrDefault(); //.ToList();


            ViewBag.statusTweetsCount = statusTweets.Count();
            ViewBag.statusTweets = statusTweets;

            ViewBag.hashTweetsCount = hashTweets.Statuses.Count();
            ViewBag.hashTweets = hashTweets.Statuses;

            return View();
        }

    }
}