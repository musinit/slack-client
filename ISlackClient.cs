using System.Collections.Generic;
using System.Threading.Tasks;
using Slack.Client.Models;
using Slack.Client.Models.SlackApi;

namespace Slack.Client
{
    public interface ISlackClient
    {
        /// <summary>
        /// Simle send message in Slack
        /// </summary>
        /// <param name="request"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        Task<bool> SendMessageAsync(SendMessageRequest request, bool isTest = true);
        
        /// <summary>
        /// Send message in response
        /// </summary>
        /// <param name="responseUrl"></param>
        /// <param name="blocks"></param>
        /// <returns></returns>
        Task SendResponseMessageAsync(string responseUrl, Block[] blocks);
        
        Task<SlackResponse> UpdateViewAsync<T>(UpdateViewRequest<T> request, string botToken, string requestUrl = "views.publish");
        
        /// <summary>
        /// Get slack user by his email
        /// </summary>
        /// <param name="email">user email</param>
        /// <returns></returns>
        Task<SlackUser> GetUserIfExistAsync(string email);

        /// <summary>
        /// Get user profile
        /// </summary>
        /// <param name="appToken"></param>
        /// <param name="slackId"></param>
        /// <returns></returns>
        Task<Profile> GetUserProfile(string appToken, string slackId);

        /// <summary>
        /// Get list of profiles
        /// </summary>
        /// <param name="appToken"></param>
        /// <param name="slackIds"></param>
        /// <returns></returns>
        Task<Profile[]> GetUserProfiles(string appToken, string[] slackIds);

        /// <summary>
        /// Get all users in workspace
        /// </summary>
        /// <returns></returns>
        Task<SlackUser[]> GetAllUsers();

        /// <summary>
        /// For getting bot token of the workspace
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        Task<OAuthResponse> Authorize(string code);
        
        /// <summary>
        /// For getting all members of the conversations
        /// </summary>
        /// <param name="conversationIds"></param>
        /// <param name="appToken"></param>
        /// <returns></returns>
        Task<string[]> GetMembersOfConversations(string[] conversationIds, string appToken);
    }
}
