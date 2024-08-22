using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebPush;

namespace ReactPWA.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class PushNotificationController : ControllerBase
    {
        private IWebHostEnvironment _hostingEnvironment;

        public PushNotificationController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost]
        public IResult Subscribe([FromBody]NotificationSubscription? notificationSubscription)
        {
            if (notificationSubscription != null)
            {
                string fileName = "subscriptions.json"; 
                string subscriptionPath = Path.Combine(_hostingEnvironment.ContentRootPath, "data");

                if (!Directory.Exists(subscriptionPath))
                {
                    Directory.CreateDirectory(subscriptionPath);
                }
                subscriptionPath = Path.Combine(subscriptionPath, fileName);

                var savedSubscriptions = System.IO.File.Exists(subscriptionPath) ?
                    JsonSerializer.Deserialize<List<NotificationSubscription>>(System.IO.File.ReadAllText(subscriptionPath)) 
                    : new List<NotificationSubscription>();

                if (savedSubscriptions == null)
                {
                    savedSubscriptions = new List<NotificationSubscription>();
                }

                savedSubscriptions.Add(notificationSubscription);

                System.IO.File.WriteAllText(subscriptionPath, JsonSerializer.Serialize(savedSubscriptions));
            }

            return Results.Ok();
        }

        [HttpPost]
        public async Task<int> SendNotification([FromBody] PushNotificationMessage message)
        {
            //Replace with your generated public/private key
            var publicKey = "BK301jmlPtPxS_ivFz4Bdi8dpCyLF0KpN1Ij_5nh5ktTb8Le8amZRuzH0JMP8ZXniBth6kse2BMQYXV8rBlFTe0";
            var privateKey = "fgHzo0wOAhhiQesuLa8D0DYf_c1H0WmMXRgotQS7CYM";

            var vapidDetails = new VapidDetails("mailto:szychov@crunchtime.com", publicKey, privateKey);
            var webPushClient = new WebPushClient();

            string fileName = "subscriptions.json"; 
            string subscriptionPath = Path.Combine(_hostingEnvironment.ContentRootPath, "data", fileName);

            if (!System.IO.File.Exists(subscriptionPath))
            {
                return 0;
            }

            var subscriptions = JsonSerializer.Deserialize<List<NotificationSubscription>>(System.IO.File.ReadAllText(subscriptionPath));

            if (subscriptions == null || !subscriptions.Any())
            {
                return 0;
            }

            var result = 0;

            var activeSubscriptions = new List<NotificationSubscription>();

            foreach (var subscription in subscriptions)
            {
                var pushSubscription = new PushSubscription(subscription.Endpoint, subscription?.Keys?.P256DH, subscription?.Keys?.Auth);

                try
                {
                    var payload = JsonSerializer.Serialize(message);
                    await webPushClient.SendNotificationAsync(pushSubscription, payload, vapidDetails);
                    if (subscription != null)
                    {
                        activeSubscriptions.Add(subscription);
                    }

                    result++;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error sending push notification: " + ex.Message);
                }
            }

            System.IO.File.WriteAllText(subscriptionPath, JsonSerializer.Serialize(activeSubscriptions));

            return result;
        }

        public class NotificationSubscription
        {
            public string? Endpoint { get; set; }
            public NotificationSubscriptionKey? Keys { get; set; }
        }

        public class NotificationSubscriptionKey
        {
            public string? Auth { get; set; }
            public string? P256DH { get; set; }
        }

        public class PushNotificationMessage
        {
            public string? openUrl { get; set; }
            public string? content { get; set; }
            public string? title { get; set; }
        }
    }
}
