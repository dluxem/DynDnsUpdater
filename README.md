DynDnsUpdate
============

A Windows command line utility to act as a pseudo-dynamic DNS
updater for Gandi.net DNS hosting.

This service relies on having your Gandi API key. This can by obtained through
their web interface under Account Management \\ API management. See below for
more information.

Download
========

Download DynDnsUpdate at http://liberum.org/DynDnsUpdate

Usage Examples
--------------

Common Parameters:

[-debug]            Turns on debug logging
[-logfile filename] Log output to filename
[-simulate]         Test and avoid permenant changes

Gandi:

`DynDnsUpdater.exe -gandi -apikey zzzzzzzzzzz -zonename example.com -hostname dynamichost`

Gandi OT&E (Test):

`DynDnsUpdater.exe -ganditest -apikey zzzzzzzzzzz -zonename example.com -hostname dynamichost`


Getting the Gandi API
---------------------

In order to use this application with Gandi DNS hosting, you will need to activate
the production API on your Gandi account.  Before that can be done, you will need to
enable the Test (OT&E) API.

Please see this page for more information: http://wiki.gandi.net/en/xml-api/activate

**WARNING:** Treat your API key as you would a password. This gives nearly full access to
your Gandi account.