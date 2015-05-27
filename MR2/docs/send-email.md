# Send Email

It is easy to send emails using MonoRail combining templates as you do with the views. Layouts are not applied to e-mail templates, though.

The most manual way to send an email is to configure a `Castle.Components.Common.EmailSender.Message` instance and invoke `DeliverEmail` which is exposed by the `Controller` class.

A better way is to externalize the configuration of the `Message`. You can do that by creating view templates on the `mail` folder under your `views` directory. You can even add headers like `from`, `to` and `subject` to the template. They will be used to properly set up the `Message` instance. The rest of the view template will be considered the e-mail body. If the body starts with an `<html>` tag, the Message format will be changed to `Html`.

The method `RenderMailMessage` can be used to create a configured `Message` instance based on the specified template. It gives you a chance to modify the message before sending it with `DeliverEmail`.

The method `RenderEmailAndSend` is a combination of the methods above. It creates the `Message` instance and sends it.