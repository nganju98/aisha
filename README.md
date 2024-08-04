# Aisha - AI-Powered Command Line Helper

Aisha is a command-line tool that uses AI to convert natural language descriptions into PowerShell or DOS commands. It leverages the Claude AI model to generate accurate commands based on user input.

# Dirt Simple
- for example, run `aisha list files by size`
- it will show you the Powershell command, and also copy it to clipboard
- Just hit `Ctrl + V` to paste it in your terminal and hit enter to run it


## Installation

1. Download the `aisha.exe` file from the latest release.

2. Choose a permanent location for `aisha.exe`. For example:
   `C:\Users\YourUsername\Tools`
   or
   `C:\Program Files\Aisha`

3. Add the chosen directory to your system's PATH:
   - Press Win + X and select "System"
   - Click on "Advanced system settings"
   - Click the "Environment Variables" button
   - In the "System variables" section, find and select the "Path" variable, then click "Edit"
   - Click "New" and add the full path to the folder containing aisha.exe
   - Click "OK" on all dialogs to save your changes

4. Restart any open command prompts or PowerShell windows for the changes to take effect.

## Usage

### Basic Usage
```
aisha <natural language command>
```
This will generate a PowerShell command based on your natural language input.

### Command-line Flags

- `-dos`: Generate a DOS command instead of PowerShell
  ```
  aisha -dos <natural language command>
  ```

### Configuration
To set up your Claude API key:
```
aisha config
```
This will prompt you to enter your API key, which will be securely stored for future use.

## Examples

1. Generate a PowerShell command:
   ```
   aisha list all files in the current directory
   Returns:
      PowerShell command:
      Get-ChildItem
      Command copied to clipboard.
   
   aisha sum of size of all files in this folder
   Returns:
      PowerShell command: (Get-ChildItem -File | Measure-Object -Property Length -Sum).Sum
      Command copied to clipboard.
    
   aisha sum of size of all files in this folder. make the result human readable with commas
   Returns:
      PowerShell command:
      (Get-ChildItem -File | Measure-Object -Property Length -Sum).Sum | ForEach-Object { "{0:N0}" -f $_ }
      Command copied to clipboard.
   ```

2. Generate a DOS command:
   ```
   aisha -dos list all files in the current directory
   ```

3. Configure the API key:
   ```
   aisha config
   ```

## Features

- Converts natural language to PowerShell or DOS commands
- Securely stores your Claude API key
- Automatically copies the generated command to your clipboard
- Works from any directory once added to PATH

## Troubleshooting

If you encounter any issues:

1. Ensure your API key is correctly configured using `aisha config`
2. Check your internet connection, as Aisha requires online access to the Claude API
3. If the command isn't recognized, make sure you've added Aisha to your PATH correctly and restarted your terminal

## Contributing

Contributions to Aisha are welcome! Please feel free to submit pull requests or create issues for bugs and feature requests.

## License

[Specify your license here]

## Disclaimer

This tool uses AI to generate commands. While it strives for accuracy, always review the generated commands before execution, especially for critical or sensitive operations.