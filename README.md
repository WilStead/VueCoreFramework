# VueCoreFramework

An ASP.NET Core API, with a Vue single-page-application front end client, tied to a SQL database back-end by Entity Framework Core.

## What is VueCoreFramework?
VueCoreFramework is a [starter project](https://github.com/WilStead/VueCoreFramework/wiki/Building-the-project) for an [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/) website with a single-page-application front end built primarily with [Vue](https://vuejs.org/).

Its [primary feature](https://github.com/WilStead/VueCoreFramework/wiki) is its ability to automatically detect and interpret [Entity Framework](https://docs.microsoft.com/en-us/ef/core/) entity classes in the application, and dynamically create pages in the client-side front end, such as data tables and [detail forms](https://github.com/WilStead/VueCoreFramework/wiki/Data-forms) for viewing and editing items. If you wanted, you could build an entire website with the VueCoreFramework by doing nothing but designing well-decorated entity classes for your database, and letting VueCoreFramework handle the rest.

Other key features include built-in [user management](https://github.com/WilStead/VueCoreFramework/wiki/User-management), user groups, and a [chat system](https://github.com/WilStead/VueCoreFramework/wiki/Chat); all of which are intended primarily to support your users' ability to control and work with the data you're exposing in an intelligent manner.

Please see [the project wiki](https://github.com/WilStead/VueCoreFramework/wiki) for more details.

## How do I use this?
The first thing you should know is that this is not a library to be included in another project. This is a starter project which is meant to be customized and further developed into your own finished website.

[The project wiki](https://github.com/WilStead/VueCoreFramework/wiki) provides detailed instructions for getting started [here](https://github.com/WilStead/VueCoreFramework/wiki/Building-the-project).

## Can I contribute?
Yes! First you should take a moment to think about what you want.

### Customization and New Features
If you want to customize the framework for your own use, then fork, clone, and build away! No need to ask. (Although when you're done, feel free to drop me a line with a link to your production site. I'd be glad to see what you do with it.)

As mentioned above, this isn't a library. That means there's no need to request that a feature be added to the original repository if there's something missing that you need for your own personal project. Just add it to your own version of the code.

### Bugfixes and Improvements
If you discover that the framework itself has a bug that needs fixing, or think it could use an improvement that would benefit all users (i.e. not a missing feature that is specific to your own use-case), then you are more than welcome to submit an [Issue](https://help.github.com/articles/about-issues/) or a [Pull request](https://help.github.com/articles/about-pull-requests/). This project is very much a work-in-progress, and your help in continuing to improve and refine it is greatly appreciated.

## Credits
VueCoreFramework was built with the essential help of libraries and technologies that are the work of many great developers, without whom it would not have been possible to create. What follows is a (probably incomplete) list:
* [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/)
* [Bootstrap](http://getbootstrap.com/)
* [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
* [jQuery](https://jquery.com/)
* [JSNLog](http://nodejs.jsnlog.com/)
* [LocalePlanet](http://www.localeplanet.com/)
* [Moment.js](http://momentjs.com/)
* [NLog](http://nlog-project.org/)
* [Typescript](http://www.typescriptlang.org/)
* [Vue](https://vuejs.org/)
* [vue-class-component](https://github.com/vuejs/vue-class-component)
* [vue-form-generator](https://github.com/icebob/vue-form-generator)
* [vue-markdown](https://github.com/miaolz123/vue-markdown)
* [vue-property-decorator](https://github.com/kaorun343/vue-property-decorator)
* [vue-router](https://github.com/vuejs/vue-router)
* [Vuetify](https://vuetifyjs.com/)
* [Vuex](https://github.com/vuejs/vuex)
* [webpack](https://github.com/webpack/webpack)