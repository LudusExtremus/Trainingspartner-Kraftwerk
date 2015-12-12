
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
var msg = request.object.get("message_text");
var pushQuery = new Parse.Query(Parse.Installation);
pushQuery.equalTo('user', targetUser);

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