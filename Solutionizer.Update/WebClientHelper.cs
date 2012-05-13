using System;
using System.Net;
using System.Threading.Tasks;

namespace Solutionizer.Update {
    public static class WebClientHelper {
        public static IWebProxy Proxy { get; set; } 

        private static WebClient CreateWebClient() {
            return new WebClient {
                Proxy = Proxy
            };
        }

        public static Task Download(Uri sourceUri, string targetPath) {
            if (sourceUri == null) {
                throw new ArgumentNullException("sourceUri", "sourceUri must not be null.");
            }
            if (String.IsNullOrWhiteSpace(targetPath)) {
                throw new ArgumentNullException("targetPath", "targetPath must not be null or empty.");
            }

            var tcs = new TaskCompletionSource<string>();

            var wc = CreateWebClient();
            wc.DownloadFileCompleted += (sender, args) => {
                if (args.Error != null) {
                    tcs.SetException(args.Error);
                } else if (args.Cancelled) {
                    tcs.SetCanceled();
                } else {
                    tcs.SetResult(targetPath);
                }
            };
            wc.DownloadFileAsync(sourceUri, targetPath);

            return tcs.Task;
        }

        
        public static Task<string> DownloadString(Uri sourceUri) {
            if (sourceUri == null) {
                throw new ArgumentNullException("sourceUri", "sourceUri must not be null.");
            }

            var tcs = new TaskCompletionSource<string>();

            var wc = new WebClient { Proxy = Proxy };
            wc.DownloadStringCompleted += (sender, args) => {
                if (args.Error != null) {
                    tcs.SetException(args.Error);
                } else if (args.Cancelled) {
                    tcs.SetCanceled();
                } else {
                    tcs.SetResult(args.Result);
                }
            };

            wc.DownloadStringAsync(sourceUri);

            return tcs.Task;
        }

    }
}