using System.Collections.Generic;
using System.Threading.Tasks;

namespace Solutionizer.Update.FeedProvider {
    public interface IUpdateFeedProvider {
        Task<List<UpdateInfo>> GetUpdateInfos();
    }
}