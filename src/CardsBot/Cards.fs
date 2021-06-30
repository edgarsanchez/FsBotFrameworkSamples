module Cards

open System.IO
open Microsoft.Bot.Schema
open Newtonsoft.Json

let createAdaptiveCardAttachment () =
    // combine path for cross platform support
    let paths = [| "."; "Resources"; "adaptiveCard.json" |]
    let adaptiveCardJson = File.ReadAllText(Path.Combine paths)

    Attachment (contentType = "application/vnd.microsoft.card.adaptive", content = JsonConvert.DeserializeObject adaptiveCardJson)

let getHeroCard () =
    HeroCard(
        title = "BotFramework Hero Card",
        subtitle = "Microsoft Bot Framework",
        text = "Build and connect intelligent bots to interact with your users naturally wherever they are, \
                from text/sms to Skype, Slack, Office 365 mail and other popular services.",
        images = ResizeArray [| CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") |],
        buttons = ResizeArray [| CardAction(ActionTypes.OpenUrl, "Get Started", value = "https://docs.microsoft.com/bot-framework") |] )

let getThumbnailCard () =
    ThumbnailCard(
        title = "BotFramework Thumbnail Card",
        subtitle = "Microsoft Bot Framework",
        text = "Build and connect intelligent bots to interact with your users naturally wherever they are, \
                from text/sms to Skype, Slack, Office 365 mail and other popular services.",
        images = ResizeArray [| CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") |],
        buttons = ResizeArray [| CardAction(ActionTypes.OpenUrl, "Get Started", value = "https://docs.microsoft.com/bot-framework") |] )

let getReceiptCard () =
    ReceiptCard(
        title = "John Doe",
        facts = ResizeArray [| Fact("Order Number", "1234"); Fact("Payment Method", "VISA 5555-****") |],
        items = ResizeArray [|
                    ReceiptItem(
                        "Data Transfer",
                        price = "$ 38.45",
                        quantity = "368",
                        image = CardImage("https://github.com/amido/azure-vector-icons/raw/master/renders/traffic-manager.png") )
                    ReceiptItem(
                        "App Service",
                        price = "$ 45.00",
                        quantity = "720",
                        image = CardImage("https://github.com/amido/azure-vector-icons/raw/master/renders/cloud-service.png") ) |],
        tax = "$ 7.50",
        total = "$ 90.95",
        buttons = ResizeArray [|
                    CardAction(
                        ActionTypes.OpenUrl,
                        "More information",
                        "https://account.windowsazure.com/content/6.10.1.38-.8225.160809-1618/aux-pre/images/offer-icon-freetrial.png",
                        value = "https://azure.microsoft.com/en-us/pricing/" ) |] )

let getSigninCard () =
    SigninCard(
        text = "BotFramework Sign-in Card",
        buttons = ResizeArray [| CardAction(ActionTypes.Signin, "Sign-in", value = "https://login.microsoftonline.com/") |] )

let getAnimationCard () =
    AnimationCard(
        title = "Microsoft Bot Framework",
        subtitle = "Animation Card",
        image = ThumbnailUrl("https://docs.microsoft.com/en-us/bot-framework/media/how-it-works/architecture-resize.png"),
        media = ResizeArray [| MediaUrl("http://i.giphy.com/Ki55RUbOV5njy.gif") |] )

let getVideoCard () =
    VideoCard(
        title = "Big Buck Bunny",
        subtitle = "by the Blender Institute",
        text = "Big Buck Bunny (code-named Peach) is a short computer-animated comedy film by the Blender Institute, \
                part of the Blender Foundation. Like the foundation's previous film Elephants Dream, \
                the film was made using Blender, a free software application for animation made by the same foundation. \
                It was released as an open-source film under Creative Commons License Attribution 3.0.",
        image = ThumbnailUrl("https://upload.wikimedia.org/wikipedia/commons/thumb/c/c5/Big_buck_bunny_poster_big.jpg/220px-Big_buck_bunny_poster_big.jpg"),
        media = ResizeArray [| MediaUrl("http://download.blender.org/peach/bigbuckbunny_movies/BigBuckBunny_320x180.mp4") |],
        buttons = ResizeArray [| CardAction(ActionTypes.OpenUrl, "Learn More", value = "https://peach.blender.org/") |] )

let getAudioCard () =
    AudioCard(
        title = "I am your father",
        subtitle = "Star Wars: Episode V - The Empire Strikes Back",
        text = "The Empire Strikes Back (also known as Star Wars: Episode V – The Empire Strikes Back \
                is a 1980 American epic space opera film directed by Irvin Kershner. Leigh Brackett and \
                Lawrence Kasdan wrote the screenplay, with George Lucas writing the film's story and serving \
                as executive producer. The second installment in the original Star Wars trilogy, it was produced \
                by Gary Kurtz for Lucasfilm Ltd. and stars Mark Hamill, Harrison Ford, Carrie Fisher, Billy Dee Williams \
                Anthony Daniels, David Prowse, Kenny Baker, Peter Mayhew and Frank Oz.",
        image = ThumbnailUrl("https://upload.wikimedia.org/wikipedia/en/3/3c/SW_-_Empire_Strikes_Back.jpg"),
        media = ResizeArray [| MediaUrl("http://www.wavlist.com/movies/004/father.wav") |],
        buttons = ResizeArray [| CardAction(ActionTypes.OpenUrl, "Read More", value = "https://en.wikipedia.org/wiki/The_Empire_Strikes_Back") |] )

let getOAuthCard () =
    OAuthCard(
        text = "BotFramework OAuth Card",
        connectionName = "OAuth connection", // Replace with the name of your Azure AD connection.
        buttons = ResizeArray [| CardAction(ActionTypes.Signin, "Sign In", value = "https://example.org/signin") |] )
