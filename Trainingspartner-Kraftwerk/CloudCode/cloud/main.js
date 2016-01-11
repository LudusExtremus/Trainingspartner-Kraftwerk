
// Use Parse.Cloud.define to define as many cloud functions as you want.
// For example:
// Parse.Cloud.define("hello", function(request, response) {
  // response.success("Hello world!");
// });

// Parse.Cloud.define('updatePartners', function(request, response) {
    // var userId = request.params.userId,
        // partners = request.params.partners;

    // var User = Parse.Object.extend('_User'),
        // user = new User({ objectId: userId });

    // user.set('partners', partners);

    // Parse.Cloud.useMasterKey();
    // user.save().then(function(user) {
        // response.success(user);
    // }, function(error) {
        // response.error(error)
    // });
// });

Parse.Cloud.afterSave("Message", function(request,response) {
	
// Creates a pointer to _User with object id of userId
var targetUser = request.object.get('receiver');
var sendingUser = request.object.get('sender');
var msg = request.object.get("message_text");
var pushQuery = new Parse.Query(Parse.Installation);
pushQuery.equalTo('user', targetUser);

request.object.get('receiver').fetch().then(function(receiver) {
	Parse.Cloud.useMasterKey();
			var newMessageFrom = receiver.get('newMessageFrom');
	
	for(var i = 0; i < newMessageFrom.length; i++) {
    if (newMessageFrom[i].toString() === sendingUser.toString()) {
        return;
		}
	}
			newMessageFrom.push(sendingUser);
			receiver.set("newMessageFrom", newMessageFrom);
            return receiver.save();
        });

		request.object.get('sender').fetch().then(function(user) {
            var sender = user.get('nick');
			var userid = request.object.get('sender').id;
            // Send push notification to query
            return Parse.Push.send({
                where: pushQuery,
                data: {
					user: userid,
					title: "Nachricht von " + sender,
                    alert: msg
                }
            });
        }).then(
            function () {
                // fetch() & Push were successful
            },
            function (error) {
                // Handle error
            }
        )
});

Parse.Cloud.afterSave("News", function(request,response) {
	
var releaseDate = request.object.get('release');
var headline = request.object.get('headline');
var pushQuery = new Parse.Query(Parse.Installation);

Parse.Push.send({
  where: pushQuery,
  data: {
	title: "Neuigkeit",
    alert: headline
  },
  push_time: releaseDate
}, {
  success: function() {
    // Push was successful
  },
  error: function(error) {
    // Handle error
  }
});
});