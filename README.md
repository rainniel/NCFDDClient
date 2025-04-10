# NCFDDClient
Nginx-CloudFlare Dynamic DNS Client (IPv6) or **NCFDDClient** for short, is a .NET application designed to automatically update the IPv6 address in **AAAA** DNS records on CloudFlare.

## My use case
I have an Nginx reverse proxy server for websites in my homelab that I want to make accessible from the internet. My option from my ISP is to use IPv6 because the IPv4 they provided me is under CGNAT. The issue with IPv6 is that it changes during service interruptions or when the router reboots. My domains in CloudFlare are set to proxy mode, making the server accessible from both IPv4 and IPv6 connections. When the server's IPv6 changes, this app will automatically update the IPv6 in the DNS records.

## How it works
![Flow chart](https://raw.githubusercontent.com/rainniel/NCFDDClient/refs/heads/main/Docs/process_flow.png)

## Requirements
Note: This application is developed and tested for Ubuntu 24.04.2, this might also work for other Linux distro.
* Ubuntu 24 or later
* Nginx
* .NET 8 Runtime
* Server with public IPv6
* Domain under CloudFlare, DNS record in proxy mode

## Installation
#### In CloudFlare account
* Get the domain Zone ID
* Create an API Token with DNS Read & DNS Edit permission

#### In server terminal
Run the commands in the terminal, in the last line, follow the instructions and provide the **Zone ID** and **API Token** from CloudFlare account
```bash
wget https://github.com/rainniel/NCFDDClient/releases/latest/download/NCFDDClient.tar.gz
tar xvzf NCFDDClient.tar.gz
cd NCFDDClient
sudo bash install.sh
```