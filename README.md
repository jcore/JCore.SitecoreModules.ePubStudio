Sitecore.ePubStudio
===================

A tool that allows ePub files creation using Sitecore content. You'll find this studio to be very similar in implementation to Sitecore APS Module.
Indeed APS allows you to go very granular on what content to use and where in generating PDF. Unfortunately, I didn't find a way to simply extend APS to generate different output format.
This tool allows you to generate book structure and html within each chapter.

The code has been tested only with Sitecore 7 and some templates use new Sitecore 7 fields.

It also depends on DotNetEpub library (https://github.com/gonzoua/DotNetEpub) which I found to be a very lean and working nicely. 
