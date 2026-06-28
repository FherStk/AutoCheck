#!/bin/bash
echo
echo "Setting up AutoCheck testing environment:"

if [ "$EUID" -eq 0 ]; then
    echo "Error: run this script as the target user, not as root."
    exit 1
fi

CURRENT_USER=$(whoami)
CURRENT_HOME="/home/$CURRENT_USER"
REPO_DIR="$CURRENT_HOME/repos/AutoCheck"
current_folder=$(pwd)

username=autocheck
password=autocheck
id -u $username &>/dev/null || sudo adduser --gecos "" --disabled-password $username
echo "$username:$password" | sudo chpasswd

sudo touch /var/lib/AccountsService/users/autocheck
echo "[User]" | sudo tee /var/lib/AccountsService/users/autocheck > /dev/null
echo "SystemAccount=true" | sudo tee -a /var/lib/AccountsService/users/autocheck > /dev/null

sudo systemctl restart accounts-daemon.service
sudo adduser $CURRENT_USER $username

sudo chown $CURRENT_USER:$username "$CURRENT_HOME"
sudo chmod 750 "$CURRENT_HOME"
sudo chown $CURRENT_USER:$username "$CURRENT_HOME/repos"
sudo chmod 750 "$CURRENT_HOME/repos"

sudo chown -R $CURRENT_USER:$username "$REPO_DIR"
sudo find "$REPO_DIR" -type d -exec chmod 2770 {} +
sudo find "$REPO_DIR" -type f -exec chmod 660 {} +
sudo find "$REPO_DIR" -name "*.sh" -exec chmod 770 {} +
sudo apt install acl -y -qq
sudo setfacl -R -d -m u::rwX,g:$username:rwX,o::--- "$REPO_DIR"

sudo -i -u $username bash -c "mkdir -p ~/repos"
sudo -i -u $username bash -c "ln -sf /home/$CURRENT_USER/repos/AutoCheck ~/repos/AutoCheck"

sudo -i -u $username bash -c "git config --global --get-all safe.directory | grep -qxF '/home/$username/repos/AutoCheck' || git config --global --add safe.directory '/home/$username/repos/AutoCheck'"

dotnet tool install -g docfx 2>/dev/null || dotnet tool update -g docfx

code --install-extension ms-dotnettools.csdevkit
code --install-extension ms-vscode-remote.remote-ssh

echo
echo "Setup complete. Please log out and log back in for the '$username' group membership to take effect."
echo "Also, copy the private files to:"
echo "  $REPO_DIR/.vscode"
echo "  $REPO_DIR/core/config"
echo "  $REPO_DIR/test/samples/private"
