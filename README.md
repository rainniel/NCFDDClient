# NCFDDClient
Nginx-CloudFlare Dynamic DNS Client (IPv6) or **NCFDDClient** for short, is a .NET application designed to automatically update the IPv6 address in **AAAA** DNS records on CloudFlare.

## My use case
I have an Nginx proxy server in my homelab that I want to make accessible from the internet. My option from my ISP is to use IPv6 because the IPv4 they provided me is under CGNAT. The issue with IPv6 is that it changes during service interruptions or when the router reboots. My domains in CloudFlare are set to proxy mode, making the server accessible from both IPv4 and IPv6 connections. When the server's IPv6 changes, this app will automatically update the IPv6 in the DNS records.

## How it works
...

## Requirements
Note: This application is developed and tested for Ubuntu 24.04.2, this might also work for other Linux distro.
* Ubuntu 24 or later
* Nginx
* Server with public IPv6
* .NET 8 Runtime

## Installation
...