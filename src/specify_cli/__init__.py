#!/usr/bin/env python3
# /// script
# requires-python = ">=3.11"
# dependencies = [
#     "typer",
#     "rich",
#     "platformdirs",
#     "readchar",
#     "httpx",
# ]
# ///
"""
Specify CLI - Setup tool for Specify projects

Usage:
    uvx specify-cli.py init <project-name>
    uvx specify-cli.py init --here
    
Or install globally:
    uv tool install --from specify-cli.py specify-cli
    specify init <project-name>
    specify init --here
"""

import os
import subprocess
import sys
import zipfile
import tempfile
import shutil
import json
from pathlib import Path
from typing import Optional

import typer
import httpx
from rich.console import Console
from rich.panel import Panel
from rich.progress import Progress, SpinnerColumn, TextColumn
from rich.text import Text
from rich.live import Live
from rich.align import Align
from rich.table import Table
from typer.core import TyperGroup

# For cross-platform keyboard input
import readchar

# Constants
AI_CHOICES = {
    "copilot": "GitHub Copilot",
    "claude": "Claude Code",
    "gemini": "Gemini CLI"
}

# ASCII Art Banner
BANNER = """
███████╗██████╗ ███████╗ ██████╗██╗███████╗██╗   ██╗
██╔════╝██╔══██╗██╔════╝██╔════╝██║██╔════╝╚██╗ ██╔╝
███████╗██████╔╝█████╗  ██║     ██║█████╗   ╚████╔╝ 
╚════██║██╔═══╝ ██╔══╝  ██║     ██║██╔══╝    ╚██╔╝  
███████║██║     ███████╗╚██████╗██║██║        ██║   
╚══════╝╚═╝     ╚══════╝ ╚═════╝╚═╝╚═╝        ╚═╝   
"""

TAGLINE = "Spec-Driven Development Scaffolding Engine"

MINI_BANNER = """
╔═╗╔═╗╔═╗╔═╗╦╔═╗╦ ╦
╚═╗╠═╝║╣ ║  ║╠╣ ╚╦╝
╚═╝╩  ╚═╝╚═╝╩╚   ╩ 
"""

def get_key():
    """Get a single keypress in a cross-platform way using readchar."""
    key = readchar.readkey()
    
    # Arrow keys
    if key == readchar.key.UP:
        return 'up'
    if key == readchar.key.DOWN:
        return 'down'
    
    # Enter/Return
    if key == readchar.key.ENTER:
        return 'enter'
    
    # Escape
    if key == readchar.key.ESC:
        return 'escape'
        
    # Ctrl+C
    if key == readchar.key.CTRL_C:
        raise KeyboardInterrupt

    return key



def select_with_arrows(options: dict, prompt_text: str = "Select an option", default_key: str = None) -> str:
    """
    Interactive selection using arrow keys with Rich Live display.
    
    Args:
        options: Dict with keys as option keys and values as descriptions
        prompt_text: Text to show above the options
        default_key: Default option key to start with
        
    Returns:
        Selected option key
    """
    option_keys = list(options.keys())
    if default_key and default_key in option_keys:
        selected_index = option_keys.index(default_key)
    else:
        selected_index = 0
    
    selected_key = None

    def create_selection_panel():
        """Create the selection panel with current selection highlighted."""
        table = Table.grid(padding=(0, 2))
        table.add_column(style="bright_cyan", justify="left", width=3)
        table.add_column(style="white", justify="left")
        
        for i, key in enumerate(option_keys):
            if i == selected_index:
                table.add_row("▶", f"[bright_cyan]{key}: {options[key]}[/bright_cyan]")
            else:
                table.add_row(" ", f"[white]{key}: {options[key]}[/white]")
        
        table.add_row("", "")
        table.add_row("", "[dim]Use ↑/↓ to navigate, Enter to select, Esc to cancel[/dim]")
        
        return Panel(
            table,
            title=f"[bold]{prompt_text}[/bold]",
            border_style="cyan",
            padding=(1, 2)
        )
    
    console.print()

    def run_selection_loop():
        nonlocal selected_key, selected_index
        with Live(create_selection_panel(), console=console, transient=True, auto_refresh=False) as live:
            while True:
                try:
                    key = get_key()
                    if key == 'up':
                        selected_index = (selected_index - 1) % len(option_keys)
                    elif key == 'down':
                        selected_index = (selected_index + 1) % len(option_keys)
                    elif key == 'enter':
                        selected_key = option_keys[selected_index]
                        break
                    elif key == 'escape':
                        console.print("\n[yellow]Selection cancelled[/yellow]")
                        raise typer.Exit(1)
                    
                    live.update(create_selection_panel(), refresh=True)

                except KeyboardInterrupt:
                    console.print("\n[yellow]Selection cancelled[/yellow]")
                    raise typer.Exit(1)

    run_selection_loop()

    if selected_key is None:
        console.print("\n[red]Selection failed.[/red]")
        raise typer.Exit(1)

    console.print(f"\n[green]✓ Selected:[/green] {selected_key}: {options[selected_key]}")
    return selected_key



console = Console()


class BannerGroup(TyperGroup):
    """Custom group that shows banner before help."""
    
    def format_help(self, ctx, formatter):
        # Show banner before help
        show_banner()
        super().format_help(ctx, formatter)


app = typer.Typer(
    name="specify",
    help="Setup tool for Specify spec-driven development projects",
    add_completion=False,
    invoke_without_command=True,
    cls=BannerGroup,
)


def show_banner():
    """Display the ASCII art banner."""
    # Create gradient effect with different colors
    banner_lines = BANNER.strip().split('\n')
    colors = ["bright_blue", "blue", "cyan", "bright_cyan", "white", "bright_white"]
    
    styled_banner = Text()
    for i, line in enumerate(banner_lines):
        color = colors[i % len(colors)]
        styled_banner.append(line + "\n", style=color)
    
    console.print(Align.center(styled_banner))
    console.print(Align.center(Text(TAGLINE, style="italic bright_yellow")))
    console.print()


@app.callback()
def callback(ctx: typer.Context):
    """Show banner when no subcommand is provided."""
    # Show banner only when no subcommand and no help flag
    # (help is handled by BannerGroup)
    if ctx.invoked_subcommand is None and "--help" not in sys.argv and "-h" not in sys.argv:
        show_banner()
        console.print(Align.center("[dim]Run 'specify --help' for usage information[/dim]"))
        console.print()


def run_command(cmd: list[str], check_return: bool = True, capture: bool = False, shell: bool = False) -> Optional[str]:
    """Run a shell command and optionally capture output."""
    try:
        if capture:
            result = subprocess.run(cmd, check=check_return, capture_output=True, text=True, shell=shell)
            return result.stdout.strip()
        else:
            subprocess.run(cmd, check=check_return, shell=shell)
            return None
    except subprocess.CalledProcessError as e:
        if check_return:
            console.print(f"[red]Error running command:[/red] {' '.join(cmd)}")
            console.print(f"[red]Exit code:[/red] {e.returncode}")
            if hasattr(e, 'stderr') and e.stderr:
                console.print(f"[red]Error output:[/red] {e.stderr}")
            raise
        return None


def check_tool(tool: str, install_hint: str) -> bool:
    """Check if a tool is installed."""
    if shutil.which(tool):
        return True
    else:
        console.print(f"[yellow]⚠️  {tool} not found[/yellow]")
        console.print(f"   Install with: [cyan]{install_hint}[/cyan]")
        return False


def is_git_repo(path: Path = None) -> bool:
    """Check if the specified path is inside a git repository."""
    if path is None:
        path = Path.cwd()
    
    if not path.is_dir():
        return False

    try:
        # Use git command to check if inside a work tree
        subprocess.run(
            ["git", "rev-parse", "--is-inside-work-tree"],
            check=True,
            capture_output=True,
            cwd=path,
        )
        return True
    except (subprocess.CalledProcessError, FileNotFoundError):
        return False


def init_git_repo(project_path: Path) -> bool:
    """Initialize a git repository in the specified path."""
    try:
        original_cwd = Path.cwd()
        os.chdir(project_path)
        
        console.print("[cyan]Initializing git repository...[/cyan]")
        subprocess.run(["git", "init"], check=True, capture_output=True)
        subprocess.run(["git", "add", "."], check=True, capture_output=True)
        subprocess.run(["git", "commit", "-m", "Initial commit from Specify template"], check=True, capture_output=True)
        
        console.print("[green]✓[/green] Git repository initialized")
        return True
        
    except subprocess.CalledProcessError as e:
        console.print(f"[red]Error initializing git repository:[/red] {e}")
        return False
    finally:
        os.chdir(original_cwd)


def download_template_from_github(ai_assistant: str, download_dir: Path) -> Path:
    """Download the latest template release from GitHub using HTTP requests."""
    repo_owner = "localden"
    repo_name = "sdd"
    
    # Get the latest release information
    console.print("[cyan]Fetching latest release information...[/cyan]")
    api_url = f"https://api.github.com/repos/{repo_owner}/{repo_name}/releases/latest"
    
    try:
        response = httpx.get(api_url, timeout=30, follow_redirects=True)
        response.raise_for_status()
        release_data = response.json()
    except httpx.RequestError as e:
        console.print(f"[red]Error fetching release information:[/red] {e}")
        raise typer.Exit(1)
    
    # Find the template asset for the specified AI assistant
    pattern = f"sdd-template-{ai_assistant}"
    matching_assets = [
        asset for asset in release_data.get("assets", [])
        if pattern in asset["name"] and asset["name"].endswith(".zip")
    ]
    
    if not matching_assets:
        console.print(f"[red]Error:[/red] No template found for AI assistant '{ai_assistant}'")
        console.print(f"[yellow]Available assets:[/yellow]")
        for asset in release_data.get("assets", []):
            console.print(f"  - {asset['name']}")
        raise typer.Exit(1)
    
    # Use the first matching asset
    asset = matching_assets[0]
    download_url = asset["browser_download_url"]
    filename = asset["name"]
    file_size = asset["size"]
    
    console.print(f"[cyan]Found template:[/cyan] {filename}")
    console.print(f"[cyan]Size:[/cyan] {file_size:,} bytes")
    console.print(f"[cyan]Release:[/cyan] {release_data['tag_name']}")
    
    # Download the file
    zip_path = download_dir / filename
    console.print(f"[cyan]Downloading template...[/cyan]")
    
    try:
        with httpx.stream("GET", download_url, timeout=30, follow_redirects=True) as response:
            response.raise_for_status()
            total_size = int(response.headers.get('content-length', 0))
            
            with open(zip_path, 'wb') as f:
                if total_size == 0:
                    # No content-length header, download without progress
                    for chunk in response.iter_bytes(chunk_size=8192):
                        f.write(chunk)
                else:
                    # Show progress bar
                    with Progress(
                        SpinnerColumn(),
                        TextColumn("[progress.description]{task.description}"),
                        TextColumn("[progress.percentage]{task.percentage:>3.0f}%"),
                        console=console,
                    ) as progress:
                        task = progress.add_task("Downloading...", total=total_size)
                        downloaded = 0
                        for chunk in response.iter_bytes(chunk_size=8192):
                            f.write(chunk)
                            downloaded += len(chunk)
                            progress.update(task, completed=downloaded)
    
    except httpx.RequestError as e:
        console.print(f"[red]Error downloading template:[/red] {e}")
        if zip_path.exists():
            zip_path.unlink()
        raise typer.Exit(1)
    
    console.print(f"[green]✓[/green] Downloaded: {filename}")
    return zip_path


def download_and_extract_template(project_path: Path, ai_assistant: str, is_current_dir: bool = False) -> Path:
    """Download the latest release and extract it to create a new project."""
    current_dir = Path.cwd()
    
    with Progress(
        SpinnerColumn(),
        TextColumn("[progress.description]{task.description}"),
        console=console,
    ) as progress:
        # Download latest release template using GitHub API
        task = progress.add_task("Setting up template download...", total=None)
        
        try:
            zip_path = download_template_from_github(ai_assistant, current_dir)
        except Exception as e:
            console.print(f"[red]Error downloading template:[/red] {e}")
            raise typer.Exit(1)
        
        progress.update(task, completed=True)
        
        # Extract ZIP file
        task = progress.add_task("Extracting template...", total=None)
        try:
            # Create project directory only if not using current directory
            if not is_current_dir:
                project_path.mkdir(parents=True)
            
            with zipfile.ZipFile(zip_path, 'r') as zip_ref:
                # List all files in the ZIP for debugging
                zip_contents = zip_ref.namelist()
                console.print(f"[cyan]ZIP contains {len(zip_contents)} items[/cyan]")
                
                # For current directory, extract to a temp location first
                if is_current_dir:
                    with tempfile.TemporaryDirectory() as temp_dir:
                        temp_path = Path(temp_dir)
                        zip_ref.extractall(temp_path)
                        
                        # Check what was extracted
                        extracted_items = list(temp_path.iterdir())
                        console.print(f"[cyan]Extracted {len(extracted_items)} items to temp location[/cyan]")
                        
                        # Handle GitHub-style ZIP with a single root directory
                        source_dir = temp_path
                        if len(extracted_items) == 1 and extracted_items[0].is_dir():
                            source_dir = extracted_items[0]
                            console.print(f"[cyan]Found nested directory structure[/cyan]")
                        
                        # Copy contents to current directory
                        for item in source_dir.iterdir():
                            dest_path = project_path / item.name
                            if item.is_dir():
                                if dest_path.exists():
                                    console.print(f"[yellow]Merging directory:[/yellow] {item.name}")
                                    # Recursively copy directory contents
                                    for sub_item in item.rglob('*'):
                                        if sub_item.is_file():
                                            rel_path = sub_item.relative_to(item)
                                            dest_file = dest_path / rel_path
                                            dest_file.parent.mkdir(parents=True, exist_ok=True)
                                            shutil.copy2(sub_item, dest_file)
                                else:
                                    shutil.copytree(item, dest_path)
                            else:
                                if dest_path.exists():
                                    console.print(f"[yellow]Overwriting file:[/yellow] {item.name}")
                                shutil.copy2(item, dest_path)
                        
                        console.print(f"[cyan]Template files merged into current directory[/cyan]")
                else:
                    # Extract directly to project directory (original behavior)
                    zip_ref.extractall(project_path)
                    
                    # Check what was extracted
                    extracted_items = list(project_path.iterdir())
                    console.print(f"[cyan]Extracted {len(extracted_items)} items to {project_path}:[/cyan]")
                    for item in extracted_items:
                        console.print(f"  - {item.name} ({'dir' if item.is_dir() else 'file'})")
                    
                    # Handle GitHub-style ZIP with a single root directory
                    if len(extracted_items) == 1 and extracted_items[0].is_dir():
                        # Move contents up one level
                        nested_dir = extracted_items[0]
                        temp_move_dir = project_path.parent / f"{project_path.name}_temp"
                        
                        # Move the nested directory contents to temp location
                        shutil.move(str(nested_dir), str(temp_move_dir))
                        # Remove the now-empty project directory
                        project_path.rmdir()
                        # Rename temp directory to project directory
                        shutil.move(str(temp_move_dir), str(project_path))
                        
                        console.print(f"[cyan]Flattened nested directory structure[/cyan]")
                    
        except Exception as e:
            console.print(f"[red]Error extracting template:[/red] {e}")
            # Clean up project directory if created and not current directory
            if not is_current_dir and project_path.exists():
                shutil.rmtree(project_path)
            raise typer.Exit(1)
        finally:
            # Clean up downloaded ZIP file
            if zip_path.exists():
                zip_path.unlink()
                console.print(f"[cyan]Cleaned up:[/cyan] {zip_path.name}")
                
        progress.update(task, completed=True)
    
    return project_path


@app.command()
def init(
    project_name: str = typer.Argument(None, help="Name for your new project directory (optional if using --here)"),
    ai_assistant: str = typer.Option(None, "--ai", help="AI assistant to use: claude, gemini, or copilot"),
    ignore_agent_tools: bool = typer.Option(False, "--ignore-agent-tools", help="Skip checks for AI agent tools like Claude Code"),
    no_git: bool = typer.Option(False, "--no-git", help="Skip git repository initialization"),
    here: bool = typer.Option(False, "--here", help="Initialize project in the current directory instead of creating a new one"),
):
    """
    Initialize a new Specify project from the latest template.
    
    This command will:
    1. Check that required tools are installed (git is optional)
    2. Let you choose your AI assistant (Claude Code, Gemini CLI, or GitHub Copilot)
    3. Download the appropriate template from GitHub
    4. Extract the template to a new project directory or current directory
    5. Initialize a fresh git repository (if not --no-git and no existing repo)
    6. Optionally set up AI assistant commands
    
    Examples:
        specify init my-project
        specify init my-project --ai claude
        specify init my-project --ai gemini
        specify init my-project --ai copilot --no-git
        specify init --ignore-agent-tools my-project
        specify init --here --ai claude
        specify init --here
    """
    # Show banner first
    show_banner()
    
    # Validate arguments
    if here and project_name:
        console.print("[red]Error:[/red] Cannot specify both project name and --here flag")
        raise typer.Exit(1)
    
    if not here and not project_name:
        console.print("[red]Error:[/red] Must specify either a project name or use --here flag")
        raise typer.Exit(1)
    
    # Determine project directory
    if here:
        project_name = Path.cwd().name
        project_path = Path.cwd()
        
        # Check if current directory has any files
        existing_items = list(project_path.iterdir())
        if existing_items:
            console.print(f"[yellow]Warning:[/yellow] Current directory is not empty ({len(existing_items)} items)")
            console.print("[yellow]Template files will be merged with existing content and may overwrite existing files[/yellow]")
            
            # Ask for confirmation
            response = typer.confirm("Do you want to continue?")
            if not response:
                console.print("[yellow]Operation cancelled[/yellow]")
                raise typer.Exit(0)
    else:
        project_path = Path(project_name).resolve()
        # Check if project directory already exists
        if project_path.exists():
            console.print(f"[red]Error:[/red] Directory '{project_name}' already exists")
            raise typer.Exit(1)
    
    console.print(Panel.fit(
        "[bold cyan]Specify Project Setup[/bold cyan]\n"
        f"{'Initializing in current directory:' if here else 'Creating new project:'} [green]{project_path.name}[/green]"
        + (f"\n[dim]Path: {project_path}[/dim]" if here else ""),
        border_style="cyan"
    ))
    
    # Check required tools
    console.print("\n[bold]Checking required tools...[/bold]")
    
    # Check git only if we might need it (not --no-git)
    git_available = True
    if not no_git:
        git_available = check_tool("git", "https://git-scm.com/downloads")
        if not git_available:
            console.print("[yellow]Git not found - will skip repository initialization[/yellow]")
    
    console.print("[green]✓[/green] All required tools are available")
    
    # AI assistant selection
    if ai_assistant:
        if ai_assistant not in AI_CHOICES:
            console.print(f"[red]Error:[/red] Invalid AI assistant '{ai_assistant}'. Choose from: {', '.join(AI_CHOICES.keys())}")
            raise typer.Exit(1)
        selected_ai = ai_assistant
    else:
        # Use arrow-key selection interface
        selected_ai = select_with_arrows(
            AI_CHOICES, 
            "Choose your AI assistant:", 
            "copilot"
        )
    
    console.print(f"[green]✓ Selected AI assistant:[/green] {AI_CHOICES[selected_ai]}")
    
    # Check agent tools unless ignored
    if not ignore_agent_tools:
        agent_tool_missing = False
        if selected_ai == "claude":
            if not check_tool("claude", "Install from: https://docs.anthropic.com/en/docs/claude-code/setup"):
                console.print("[red]Error:[/red] Claude CLI is required for Claude Code projects")
                agent_tool_missing = True
        elif selected_ai == "gemini":
            if not check_tool("gemini", "Install from: https://github.com/google-gemini/gemini-cli"):
                console.print("[red]Error:[/red] Gemini CLI is required for Gemini projects")
                agent_tool_missing = True
        # GitHub Copilot check is not needed as it's typically available in supported IDEs
        
        if agent_tool_missing:
            console.print("\n[red]Required AI tool is missing![/red]")
            console.print("[yellow]Tip:[/yellow] Use --ignore-agent-tools to skip this check")
            raise typer.Exit(1)
    
    # Download and set up project
    console.print("\n[bold]Setting up project from latest template...[/bold]")
    
    try:
        download_and_extract_template(project_path, selected_ai, here)
        
        # Handle git repository initialization
        if not no_git:
            if is_git_repo(project_path):
                console.print(f"[yellow]ℹ️[/yellow] Existing git repository detected")
                console.print("[green]✓[/green] Skipping git initialization")
            elif git_available:
                if init_git_repo(project_path):
                    pass  # Success message already printed in init_git_repo
                else:
                    console.print("[yellow]⚠️  Git repository initialization failed, but project was created successfully[/yellow]")
            else:
                console.print("[yellow]⚠️  Git not available - skipping repository initialization[/yellow]")
                console.print("[yellow]   You can initialize git later with: git init[/yellow]")
        else:
            console.print("[cyan]ℹ️[/cyan] Git initialization skipped (--no-git flag)")
        
    except Exception as e:
        console.print(f"[red]Failed to set up project:[/red] {e}")
        # Clean up partial directory if it was created and not current directory
        if not here and project_path.exists():
            shutil.rmtree(project_path)
        raise typer.Exit(1)
    
    # Success!
    console.print("\n" + "─" * 60)
    success_text = Text("✨ Success! ", style="green bold") + Text("Your Specify project is ready.", style="white")
    console.print(Align.center(success_text))
    console.print("─" * 60)
    
    console.print("\n[bold]Next steps:[/bold]")
    if not here:
        console.print(f"1. [cyan]cd {project_name}[/cyan]")
        step_num = "2"
    else:
        console.print("1. You're already in the project directory!")
        step_num = "2"
    
    if selected_ai == "claude":
        console.print(f"{step_num}. Open in VSCode and start using / commands with Claude Code")
        console.print("   - Type / in any file to see available commands")
        console.print("   - Use [cyan]/spec[/cyan] to create specifications")
        console.print("   - Use [cyan]/plan[/cyan] to create implementation plans")
        console.print("   - Use [cyan]/tasks[/cyan] to generate tasks")
    elif selected_ai == "gemini":
        console.print(f"{step_num}. Use @ commands with Gemini CLI")
        console.print("   - Run [cyan]gemini @spec[/cyan] to create specifications")
        console.print("   - Run [cyan]gemini @plan[/cyan] to create implementation plans")
        console.print("   - See [cyan]GEMINI.md[/cyan] for all available commands")
    elif selected_ai == "copilot":
        console.print(f"{step_num}. Open in VSCode and use natural language with GitHub Copilot")
        console.print("   - See .github/copilot-instructions.md for available commands")
        console.print("   - Use Copilot Chat for interactive assistance")
    
    next_step = str(int(step_num) + 1)
    console.print(f"{next_step}. Read README.md for project overview")
    console.print(f"{str(int(next_step) + 1)}. Check the documentation in the docs/ folder")
    
    console.print("\n[italic bright_yellow]Happy coding with Specify! 🚀[/italic bright_yellow]\n")


@app.command()
def check():
    """Check that all required tools are installed."""
    show_banner()
    console.print("[bold]Checking Specify requirements...[/bold]\n")
    
    # Check if we have internet connectivity by trying to reach GitHub API
    console.print("[cyan]Checking internet connectivity...[/cyan]")
    try:
        response = httpx.get("https://api.github.com", timeout=5, follow_redirects=True)
        console.print("[green]✓[/green] Internet connection available")
    except httpx.RequestError:
        console.print("[red]✗[/red] No internet connection - required for downloading templates")
        console.print("[yellow]Please check your internet connection[/yellow]")
    
    console.print("\n[cyan]Optional tools:[/cyan]")
    git_ok = check_tool("git", "https://git-scm.com/downloads")
    
    console.print("\n[cyan]Optional AI tools:[/cyan]")
    claude_ok = check_tool("claude", "Install from: https://docs.anthropic.com/en/docs/claude-code/setup")
    gemini_ok = check_tool("gemini", "Install from: https://github.com/google-gemini/gemini-cli")
    
    console.print("\n[green]✓ Specify CLI is ready to use![/green]")
    if not git_ok:
        console.print("[yellow]Consider installing git for repository management[/yellow]")
    if not (claude_ok or gemini_ok):
        console.print("[yellow]Consider installing an AI assistant for the best experience[/yellow]")


def main():
    app()


if __name__ == "__main__":
    main()