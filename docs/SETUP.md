# Building and Setup

This guide walks you through setting up a local spikewall development environment
from scratch. It targets **Linux / WSL2 (Ubuntu)**, with notes for other platforms
where relevant.

If you just want the short version, jump to the [Quick Start](#quick-start).

---

## Requirements

| Component            | Version / Notes                                                        |
| -------------------- | --------------------------------------------------------------------- |
| .NET SDK             | **9.0** (the project targets `net9.0`)                                 |
| MySQL-compatible DB  | **MariaDB** (recommended) or **MySQL** — connected via `MySqlConnector`|
| OS                   | Linux, macOS, or Windows (incl. WSL2)                                  |
| A Sonic Runners client | Required only for real gameplay / integration testing               |

### Recommended tooling for contributors

- **Visual Studio Code** with the **C# Dev Kit** extension
  (`ms-dotnettools.csdevkit`), which pulls in the C# extension automatically.
  JetBrains Rider or Visual Studio also work (a `.sln.DotSettings` is included).
- On **WSL/remote**, also install the **WSL** extension
  (`ms-vscode-remote.remote-wsl`) on the Windows side.
- Optional: a database GUI such as the **MySQL client** extension
  (`cweijan.vscode-mysql-client2`) to inspect the `sw_*` tables.
- **Git** for version control.

---

## 1. Install the .NET 9 SDK

> **Heads up (WSL / Ubuntu):** .NET 9 is a Standard-Term-Support release and reached
> end-of-life in 2026. It is therefore **no longer available via `apt`** on recent
> Ubuntu releases (which only carry the LTS versions, e.g. 8.0 and 10.0). Use
> Microsoft's install script instead, as shown below.

### Linux / WSL (recommended: official install script)

Installs the SDK into `~/.dotnet` without touching system packages:

```bash
curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
chmod +x /tmp/dotnet-install.sh
/tmp/dotnet-install.sh --channel 9.0
```

Add it to your shell PATH so terminals can find it:

```bash
echo '' >> ~/.bashrc
echo '# .NET SDK' >> ~/.bashrc
echo 'export DOTNET_ROOT="$HOME/.dotnet"' >> ~/.bashrc
echo 'export PATH="$HOME/.dotnet:$PATH"' >> ~/.bashrc
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$HOME/.dotnet:$PATH"
```

> **WSL gotcha — make `dotnet` visible to VS Code extensions.**
> Adding `dotnet` only to `~/.bashrc` makes it work in your **interactive terminal**
> but **not** for processes spawned by GUI apps such as the C# Dev Kit, which launch
> via `/bin/sh` and do **not** read `~/.bashrc`. If the C# extension reports
> *"The .NET SDK cannot be located"* or `LanguageServerProjectLoader` errors, create
> a system-wide symlink so every process can find it, then reload the VS Code window
> (`Ctrl+Shift+P` → **Developer: Reload Window**):
>
> ```bash
> sudo ln -sf "$HOME/.dotnet/dotnet" /usr/local/bin/dotnet
> ```

### Other platforms

- **Windows / macOS:** download the .NET 9 SDK installer from
  <https://dotnet.microsoft.com/download/dotnet/9.0>.
- **Distros that still package it:** `sudo apt install -y dotnet-sdk-9.0`
  (only if your feed still provides it).

Verify the install:

```bash
dotnet --version      # should print 9.0.x
dotnet --list-sdks
```

---

## 2. Install and run the database (MariaDB)

```bash
sudo apt install -y mariadb-server
sudo systemctl enable --now mariadb     # start now and on every boot
sudo mariadb-secure-installation        # interactive hardening wizard
```

Recommended answers for `mariadb-secure-installation` on a fresh install:

| Prompt                                   | Answer | Why                                             |
| ---------------------------------------- | ------ | ----------------------------------------------- |
| Enter current password for root          | *(Enter)* | No password yet on a fresh install           |
| Switch to unix_socket authentication     | `Y`    | Lets `sudo mariadb` work; secure default        |
| Change the root password                 | `n`    | Not needed with unix_socket auth                |
| Remove anonymous users                   | `Y`    | Security                                        |
| Disallow root login remotely             | `Y`    | Security                                        |
| Remove test database                     | `Y`    | Security                                        |
| Reload privilege tables                  | `Y`    | Apply changes immediately                       |

> On WSL, ensure systemd is enabled (`/etc/wsl.conf` contains `systemd=true` under
> `[boot]`) so `systemctl` can manage the service. Otherwise start it manually with
> `sudo service mariadb start`.

> If you prefer stock **MySQL** instead: `sudo apt install -y mysql-server`, then
> `sudo systemctl enable --now mysql` and `sudo mysql_secure_installation`.

### Create the database and a dedicated user

Replace `YOUR_PASSWORD` with a password of your choice:

```bash
sudo mariadb -e "CREATE DATABASE spikewall;
CREATE USER 'spikewall'@'localhost' IDENTIFIED BY 'YOUR_PASSWORD';
GRANT ALL PRIVILEGES ON spikewall.* TO 'spikewall'@'localhost';
FLUSH PRIVILEGES;"
```

Verify the new credentials work before continuing (this catches password typos
early — the value you set here must match the one you send in step 4a):

```bash
mariadb -u spikewall -pYOUR_PASSWORD spikewall -e "SELECT 1;"
```

A `1` in the output means the account can log in. Keep these values handy — you'll
register them with the server in step 4:

| Setting  | Value        |
| -------- | ------------ |
| host     | `127.0.0.1`  |
| port     | `3306`       |
| username | `spikewall`  |
| password | `YOUR_PASSWORD` |
| database | `spikewall`  |

---

## 3. Build and run the server

From the repository root:

```bash
dotnet build      # optional: verify it compiles
dotnet run
```

The server listens on **`http://0.0.0.0:9001`** (configured in
[`appsettings.json`](../appsettings.json)). In the **Development** environment,
Swagger UI is available at `http://localhost:9001/swagger`.

On first launch you'll see:

```
No database details were found. Please use /Dashboard/setDatabaseDetails to update them.
```

That's expected — continue to the next step.

---

## 4. Register the database and initialize tables

Database connection details are stored **encrypted on disk** (via ASP.NET Data
Protection), not in a config file, so you register them once through an endpoint.

### 4a. Register the connection

Send a POST to `/Dashboard/setDatabaseDetails`. The endpoint reads its parameters
from the **query string**, so pass them there — with `curl`, the `-G` flag sends the
`--data-urlencode` values as query parameters (while still encoding them safely):

```bash
curl -G -X POST "http://localhost:9001/Dashboard/setDatabaseDetails" \
  --data-urlencode "host=127.0.0.1" \
  --data-urlencode "port=3306" \
  --data-urlencode "username=spikewall" \
  --data-urlencode "password=YOUR_PASSWORD" \
  --data-urlencode "database=spikewall"
```

> Using **Swagger UI** (`http://localhost:9001/swagger`) is the easiest option — it
> submits the fields in the format the endpoint expects. If you send the values as a
> form body instead of the query string (e.g. `curl` without `-G`), the server
> responds `400 "The <field> field is required"`.

A `200` response means the credentials were accepted and saved. They persist across
restarts, so this is a one-time step per machine.

### 4b. Create and seed the tables

The schema is created by the `resetDatabase` endpoint. Each boolean flag
initializes a group of tables; pass the ones you want (they DROP and recreate, so
only run against a database you don't mind wiping):

```bash
curl "http://localhost:9001/Dashboard/resetDatabase?chao=true&players=true&characters=true&mileageMapStates=true&config=true&tickers=true&dailyChallenge=true&costs=true&itemOwnership=true&information=true&incentives=true&wheelOptions=true&itemRouletteOptions=true"
```

The `config` table also auto-creates itself on first access if missing.

You're now ready to point a Sonic Runners client at the server.

---

## Quick Start

For an already-configured machine:

```bash
# 1. Ensure the DB is running
sudo systemctl start mariadb

# 2. Run the server
dotnet run

# 3. (First time only) register DB details and create tables — see step 4
```

---

## Troubleshooting

| Symptom                                                        | Fix                                                                                                   |
| -------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------- |
| `Unable to locate package dotnet-sdk-9.0`                      | .NET 9 is EOL and dropped from apt — use the install script in [step 1](#1-install-the-net-9-sdk).     |
| C# Dev Kit: *"The .NET SDK cannot be located"* / `dotnet: not found` | Symlink dotnet system-wide: `sudo ln -sf ~/.dotnet/dotnet /usr/local/bin/dotnet`, then reload the window. |
| `No database details were found`                               | Expected on first run — complete [step 4](#4-register-the-database-and-initialize-tables).             |
| `setDatabaseDetails` returns `400 "The <field> field is required"` | Pass the parameters in the **query string**, not a form body — use `curl -G` or Swagger UI (see [step 4a](#4a-register-the-connection)). |
| `Access denied for user 'spikewall'@'localhost' (using password: YES)` | The password doesn't match. Reset it to match step 4a: `sudo mariadb -e "ALTER USER 'spikewall'@'localhost' IDENTIFIED BY 'YOUR_PASSWORD'; FLUSH PRIVILEGES;"` |
| `Failed to connect to MySQL with stored details`               | Verify MariaDB is running and the credentials/database from step 2 are correct.                        |
| `systemctl` fails on WSL                                       | Enable systemd (`/etc/wsl.conf` → `[boot]` `systemd=true`) or use `sudo service mariadb start`.        |

---

## Security notes

- The `/Dashboard/*` endpoints are currently **unauthenticated** (see the `FIXME`
  markers in [`DashboardController.cs`](../Controllers/DashboardController.cs)). Do
  **not** expose them publicly; restrict access at the reverse proxy / firewall.
- Disable the client debug endpoints in production by keeping
  `enable_debug_endpoints = 0` in the `sw_config` table (the default).
- Outside the Development environment, Swagger is automatically disabled.
