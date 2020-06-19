<img src="https://image.flaticon.com/icons/svg/906/906360.svg" alt="alt text" width="50" height="50"> .NET Core Slack client
=======

## Features
- Works with slack-blocks - modern rich layout. Check it here https://api.slack.com/block-kit
- Handles slack responses, modal-views with updating existing modals in stack-manner, works with different workplaces
- Uses v2 of Slack authorization (OAuth2)
- Uses modern conversations api (not everywhere yet)
- Easy to install and use with DI

## How to install
    services.AddSlackClient(options => {
        options.SlackApi = "API of your workplace (like 'workplace_name.slack.com/api')";
        options.AppToken = "Token of your app, it's different for any workplace";
        options.ClientId = "It was given you after you Slack App was installed";
        options.ClientSecret = "The same as for the ClientId";
    })
Then you can simple inject ISlackClient where you need
    
## Feedback
Please, write me on olegmusinem@gmail.com!