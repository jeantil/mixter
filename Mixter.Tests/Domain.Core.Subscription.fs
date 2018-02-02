﻿namespace Mixter.Tests.Domain.Core.Subscription

open Xunit
open Swensen.Unquote
open Mixter.Domain.Identity.UserIdentity
open Mixter.Domain.Core.Message
open Mixter.Domain.Core.Subscription

module ``Subscription should`` =
    [<Fact>] 
    let ``When follow Then UserFollowed is returned`` () =
        let follower = { Email = "follower@mix-it.fr" } 
        let followee = { Email = "followee@mix-it.fr" }

        test <@ follow follower followee
                    = [UserFollowed { SubscriptionId = { Follower = follower; Followee = followee } }] @>

    [<Fact>] 
    let ``When unfollow Then UserUnfollowed is returned`` () =
        let subscription = { Follower = { Email = "follower@mix-it.fr" }; Followee = { Email = "followee@mix-it.fr" } }
        let history = [UserFollowed { SubscriptionId = subscription }]

        test 
          <@ history |> unfollow
                = [UserUnfollowed { SubscriptionId = subscription }] @>

    [<Fact>] 
    let ``When notify follower Then FolloweeMessageQuacked`` () =
        let subscription = { Follower = { Email = "follower@mix-it.fr" }; Followee = { Email = "followee@mix-it.fr"} }
        let message = MessageId.Generate()
        let history = [UserFollowed { SubscriptionId = subscription}]

        test <@ history |> notifyFollower message
                    = [FolloweeMessageQuacked { SubscriptionId = subscription; Message = message}] @>

    [<Fact>] 
    let ``Given unfollow When notify follower Then do not returned FollowerMessageQuacked`` () =
        let subscription = { Follower = { Email = "follower@mix-it.fr"}; Followee = { Email = "followee@mix-it.fr"} }
        let message = MessageId.Generate()
        let history = [
            UserFollowed { SubscriptionId = subscription}
            UserUnfollowed { SubscriptionId = subscription }
        ]   
        
        test <@ history |> notifyFollower message |> Seq.isEmpty @>