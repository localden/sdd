#!/usr/bin/env python3
# /// script
# requires-python = ">=3.11"
# dependencies = [
#     "typer",
#     "rich",
#     "platformdirs",
#     "readchar",
# ]
# ///
"""
Specify CLI - Setup tool for Specify projects

Usage:
    uvx specify-cli.py init <project-name>
    
Or install globally:
    uv tool install --from specify-cli.py specify-cli
    specify init <project-name>
"""

import os
import subprocess
import sys
import zipfile
import tempfile
import shutil
from pathlib import Path
from typing import Optional

import typer
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
    "claude": "Claude Code",
    "gemini": "Gemini CLI", 
    "copilot": "GitHub Copilot"
}

CRITICAL_TOOLS = [
    ("gh", "https://cli.github.com/")
]

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


def download_and_extract_template(project_name: str, ai_assistant: str) -> Path:
    """Download the latest release and extract it to create a new project."""
    project_path = Path(project_name).resolve()
    
    # Note: Directory existence is already checked in the init command
    
    with Progress(
        SpinnerColumn(),
        TextColumn("[progress.description]{task.description}"),
        console=console,
    ) as progress:
        # Create temporary directory for download
        with tempfile.TemporaryDirectory() as temp_dir:
            temp_path = Path(temp_dir)
            
            # Download latest release template using gh CLI
            task = progress.add_task("Downloading latest template...", total=None)
            try:
                # Use gh to download the specific AI assistant template ZIP file
                pattern = f"sdd-template-{ai_assistant}-*.zip"
                run_command([
                    "gh", "release", "download", 
                    "--repo", "localden/sdd",
                    "--pattern", pattern,
                    "--dir", str(temp_path)
                ])
            except subprocess.CalledProcessError as e:
                console.print(f"[red]Error downloading template:[/red] {e}")
                raise typer.Exit(1)
            progress.update(task, completed=True)
            
            # Find the downloaded template ZIP file
            pattern = f"sdd-template-{ai_assistant}-*.zip"
            template_files = list(temp_path.glob(pattern))
            if not template_files:
                console.print("[red]Error:[/red] No template ZIP file found after download")
                console.print(f"[yellow]Looking for files matching pattern: {pattern}[/yellow]")
                console.print(f"[yellow]Files found in {temp_path}:[/yellow]")
                for file in temp_path.iterdir():
                    console.print(f"  - {file.name}")
                raise typer.Exit(1)
            
            zip_path = template_files[0]
            console.print(f"[cyan]Downloaded:[/cyan] {zip_path.name}")
            console.print(f"[cyan]Size:[/cyan] {zip_path.stat().st_size:,} bytes")
            
            # Extract ZIP file
            task = progress.add_task("Extracting template...", total=None)
            try:
                with zipfile.ZipFile(zip_path, 'r') as zip_ref:
                    # List all files in the ZIP for debugging
                    zip_contents = zip_ref.namelist()
                    console.print(f"[cyan]ZIP contains {len(zip_contents)} items[/cyan]")
                    
                    # Extract to temporary directory first
                    extract_dir = temp_path / "extracted"
                    zip_ref.extractall(extract_dir)
                    
                    # Check what was extracted
                    extracted_items = list(extract_dir.iterdir())
                    console.print(f"[cyan]Extracted {len(extracted_items)} items:[/cyan]")
                    for item in extracted_items:
                        console.print(f"  - {item.name} ({'dir' if item.is_dir() else 'file'})")
                    
                    # Handle different ZIP structures
                    if len(extracted_items) == 1 and extracted_items[0].is_dir():
                        # GitHub-style ZIP with a single root directory
                        extracted_source = extracted_items[0]
                    elif len(extracted_items) > 0:
                        # Our release ZIP with direct contents
                        extracted_source = extract_dir
                    else:
                        console.print("[red]Error:[/red] ZIP file appears to be empty")
                        raise typer.Exit(1)
                    
                    # Move contents to project directory
                    project_path.mkdir(parents=True)
                    for item in extracted_source.iterdir():
                        if item.is_dir():
                            shutil.copytree(item, project_path / item.name)
                        else:
                            shutil.copy2(item, project_path / item.name)
                            
            except Exception as e:
                console.print(f"[red]Error extracting template:[/red] {e}")
                raise typer.Exit(1)
                
            progress.update(task, completed=True)
    
    return project_path


@app.command()
def init(
    project_name: str = typer.Argument(help="Name for your new project directory"),
    ai_assistant: str = typer.Option(None, "--ai", help="AI assistant to use: claude, gemini, or copilot"),
    ignore_agent_tools: bool = typer.Option(False, "--ignore-agent-tools", help="Skip checks for AI agent tools like Claude Code"),
    no_git: bool = typer.Option(False, "--no-git", help="Skip git repository initialization"),
):
    """
    Initialize a new Specify project from the latest template.
    
    This command will:
    1. Check that required tools are installed (git (optional), gh)
    2. Let you choose your AI assistant (Claude Code, Gemini CLI, or GitHub Copilot)
    3. Download the appropriate template from GitHub
    4. Extract the template to a new project directory
    5. Initialize a fresh git repository (if not --no-git and no existing repo)
    6. Optionally set up AI assistant commands
    
    Examples:
        specify init my-project
        specify init my-project --ai claude
        specify init my-project --ai gemini
        specify init my-project --ai copilot --no-git
        specify init --ignore-agent-tools my-project
    """
    # Show banner first
    show_banner()
    
    console.print(Panel.fit(
        "[bold cyan]Specify Project Setup[/bold cyan]\n"
        f"Creating new project: [green]{project_name}[/green]",
        border_style="cyan"
    ))
    
    # Check if project directory already exists
    if os.path.exists(project_name):
        console.print(f"[red]Error:[/red] Directory '{project_name}' already exists")
        raise typer.Exit(1)
    
    # Check required tools
    console.print("\n[bold]Checking required tools...[/bold]")
    
    # Check git only if we might need it (not --no-git)
    git_available = True
    if not no_git:
        git_available = check_tool("git", "https://git-scm.com/downloads")
    
    # Check gh CLI (always required for template download)
    gh_available = check_tool("gh", "https://cli.github.com/")
    
    if not gh_available:
        console.print("\n[red]GitHub CLI (gh) is required for downloading templates![/red]")
        console.print("[red]Cannot proceed without gh.[/red]")
        raise typer.Exit(1)
    
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
            "claude"
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
        download_and_extract_template(project_name, selected_ai)
        
        # Handle git repository initialization
        if not no_git:
            project_absolute_path = Path(project_name).resolve()
            
            if is_git_repo(project_absolute_path):
                console.print(f"[yellow]ℹ️[/yellow] Existing git repository detected in {project_name}")
                console.print("[green]✓[/green] Skipping git initialization")
            elif git_available:
                if init_git_repo(project_absolute_path):
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
        # Clean up partial directory if it was created
        if os.path.exists(project_name):
            shutil.rmtree(project_name)
        raise typer.Exit(1)
    
    # Success!
    console.print("\n" + "─" * 60)
    success_text = Text("✨ Success! ", style="green bold") + Text("Your Specify project is ready.", style="white")
    console.print(Align.center(success_text))
    console.print("─" * 60)
    
    console.print("\n[bold]Next steps:[/bold]")
    console.print(f"1. [cyan]cd {project_name}[/cyan]")
    
    if selected_ai == "claude":
        console.print("2. Open in VSCode and start using / commands with Claude Code")
        console.print("   - Type / in any file to see available commands")
        console.print("   - Use [cyan]/spec[/cyan] to create specifications")
        console.print("   - Use [cyan]/plan[/cyan] to create implementation plans")
        console.print("   - Use [cyan]/tasks[/cyan] to generate tasks")
    elif selected_ai == "gemini":
        console.print("2. Use @ commands with Gemini CLI")
        console.print("   - Run [cyan]gemini @spec[/cyan] to create specifications")
        console.print("   - Run [cyan]gemini @plan[/cyan] to create implementation plans")
        console.print("   - See [cyan]GEMINI.md[/cyan] for all available commands")
    elif selected_ai == "copilot":
        console.print("2. Open in VSCode and use natural language with GitHub Copilot")
        console.print("   - See .github/copilot-instructions.md for available commands")
        console.print("   - Use Copilot Chat for interactive assistance")
    
    console.print("3. Read README.md for project overview")
    console.print("4. Check the documentation in the docs/ folder")
    
    console.print("\n[italic bright_yellow]Happy coding with Specify! 🚀[/italic bright_yellow]\n")


@app.command()
def check():
    """Check that all required tools are installed."""
    show_banner()
    console.print("[bold]Checking Specify requirements...[/bold]\n")
    
    # Check critical tools
    all_ok = all(check_tool(tool, hint) for tool, hint in CRITICAL_TOOLS)
    all_ok &= check_tool("uv", "curl -LsSf https://astral.sh/uv/install.sh | sh")
    
    console.print("\n[cyan]Optional tools:[/cyan]")
    git_ok = check_tool("git", "https://git-scm.com/downloads")
    
    console.print("\n[cyan]Optional AI tools:[/cyan]")
    claude_ok = check_tool("claude", "Install from: https://docs.anthropic.com/en/docs/claude-code/setup")
    gemini_ok = check_tool("gemini", "Install from: https://github.com/google-gemini/gemini-cli")
    
    if all_ok:
        console.print("\n[green]✓ All required tools installed![/green]")
        if not git_ok:
            console.print("[yellow]Consider installing git for repository management[/yellow]")
        if not (claude_ok or gemini_ok):
            console.print("[yellow]Consider installing an AI assistant for the best experience[/yellow]")
    else:
        console.print("\n[red]✗ Some required tools are missing[/red]")
        raise typer.Exit(1)


def main():
    app()


if __name__ == "__main__":
    main()