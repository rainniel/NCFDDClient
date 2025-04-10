#!/bin/bash
if [[ $EUID -ne 0 ]]; then
	echo "This script must be run as root" 
	exit 1
fi

service_name="ncfddclient"
service_file="/etc/systemd/system/${service_name}.service"

if systemctl list-units --type=service --state=loaded | grep -q "${service_name}.service"; then
	read -p "NCFDDClient service is installed, uninstall the service? [y/N] " input

	if [[ "$input" =~ ^[Yy]$ ]]; then
		echo "Uninstalling the service..."
		systemctl stop $service_name
		systemctl disable $service_name
		rm $service_file
		systemctl daemon-reload
		echo "Service uninstalled."
	fi
else
	read -p "NCFDDClient service is not installed, install the service? [Y/n] " input

	if [[ "$input" =~ ^[Yy]$ || -z "$input" ]]; then
		if ! dpkg -l | grep -q 'aspnetcore-runtime-8.0'; then
			echo "Installing .NET 8 Runtime"
			apt-get update
			apt-get install -y aspnetcore-runtime-8.0

			if ! dpkg -l | grep -q 'aspnetcore-runtime-8.0'; then
				echo ".NET 8 Runtime installation failed, service installation cancelled."
				exit 1
			fi
		fi

		if test -f $service_file; then
			read -p "The service file is already exist, do you want to ovewrite it? [y/N]: " input
			if [[ "$input" =~ ^[Yy]$ ]]; then
				rm $service_file
			else
				echo "Service installation cancelled."
				exit 1
			fi
		fi

		script_dir=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" &> /dev/null && pwd)
		printf "[Unit]\n" >> $service_file
		printf "Description=Nginx-CloudFlare Dynamic DNS Client (IPv6)\n" >> $service_file
		printf "After=network.target\n\n" >> $service_file
		printf "[Service]\n" >> $service_file
		printf "ExecStart=/usr/bin/dotnet ${script_dir}/NCFDDClient.dll\n" >> $service_file
		printf "WorkingDirectory=${script_dir}\n" >> $service_file
		printf "Restart=no\n" >> $service_file
		printf "User=root\n\n" >> $service_file
		printf "[Install]\n" >> $service_file
		printf "WantedBy=multi-user.target" >> $service_file

		env_file="${script_dir}/.env"
		if test -f $env_file; then
			read -p "The .env file is already exist, do you want to ovewrite it? [y/N]: " input
			if [[ "$input" =~ ^[Yy]$ ]]; then
				rm $env_file
				read -p "Enter CloudFlare Zone ID: " input
				printf "cf_zone_id=${input}\n" >> $env_file
				read -p "Enter CloudFlare API Token: " input
				printf "cf_api_token=${input}" >> $env_file
			fi
		else
			read -p "Enter CloudFlare Zone ID: " input
			printf "cf_zone_id=${input}\n" >> $env_file
			read -p "Enter CloudFlare API Token: " input
			printf "cf_api_token=${input}" >> $env_file
		fi

		echo "Enabling & starting service..."
		systemctl daemon-reload
		systemctl enable $service_name
		systemctl start $service_name
		echo "Service installed & started."
	else
		echo "Service installation cancelled."
	fi
fi