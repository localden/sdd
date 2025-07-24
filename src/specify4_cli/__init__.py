#!/usr/bin/env python3
# /// script
# requires-python = ">=3.11"
# dependencies = [
#     "typer",
#     "rich",
#     "platformdirs",
# ]
# ///
"""
Specify CLI - Setup tool for Specify4 projects

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
from rich.align import Align
from typer.core import TyperGroup

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

console = Console()


class BannerGroup(TyperGroup):
    """Custom group that shows banner before help."""
    
    def format_help(self, ctx, formatter):
        # Show banner before help
        show_banner()
        super().format_help(ctx, formatter)


app = typer.Typer(
    name="specify",
    help="Setup tool for Specify4 spec-driven development projects",
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


def run_command(cmd: list[str], check: bool = True, capture: bool = False, shell: bool = False) -> Optional[str]:
    """Run a shell command and optionally capture output."""
    try:
        if capture:
            result = subprocess.run(cmd, check=check, capture_output=True, text=True, shell=shell)
            return result.stdout.strip()
        else:
            subprocess.run(cmd, check=check, shell=shell)
            return None
    except subprocess.CalledProcessError as e:
        if check:
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


def check_claude_code() -> bool:
    """Check if Claude Code is installed and configured."""
    claude_installed = check_tool("claude", "Visit https://claude.ai/code to install Claude Code")
    
    if claude_installed:
        # Check if Claude Code has been initialized
        claude_dir = Path.home() / ".claude"
        if not claude_dir.exists():
            console.print("[yellow]⚠️  Claude Code not initialized[/yellow]")
            console.print("   Run [cyan]claude[/cyan] once to set up")
            return False
            
    return claude_installed


def download_and_extract_template(project_name: str) -> Path:
    """Download the latest release and extract it to create a new project."""
    project_path = Path(project_name).resolve()
    
    if project_path.exists():
        console.print(f"[red]Error:[/red] Directory '{project_name}' already exists")
        raise typer.Exit(1)
    
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
                # Use gh to download the specific template ZIP file
                run_command([
                    "gh", "release", "download", 
                    "--repo", "localden/sdd",
                    "--pattern", "sdd-template-*.zip",
                    "--dir", str(temp_path)
                ])
            except subprocess.CalledProcessError as e:
                console.print(f"[red]Error downloading template:[/red] {e}")
                raise typer.Exit(1)
            progress.update(task, completed=True)
            
            # Find the downloaded template ZIP file
            template_files = list(temp_path.glob("sdd-template-*.zip"))
            if not template_files:
                console.print("[red]Error:[/red] No template ZIP file found after download")
                console.print("[yellow]Looking for files matching pattern: sdd-template-*.zip[/yellow]")
                raise typer.Exit(1)
            
            zip_path = template_files[0]
            
            # Extract ZIP file
            task = progress.add_task("Extracting template...", total=None)
            try:
                with zipfile.ZipFile(zip_path, 'r') as zip_ref:
                    # Extract to temporary directory first
                    extract_dir = temp_path / "extracted"
                    zip_ref.extractall(extract_dir)
                    
                    # Find the extracted folder (GitHub ZIP creates a folder like "localden-sdd-{commit}")
                    extracted_items = list(extract_dir.iterdir())
                    if len(extracted_items) != 1 or not extracted_items[0].is_dir():
                        console.print("[red]Error:[/red] Unexpected ZIP structure")
                        raise typer.Exit(1)
                    
                    extracted_source = extracted_items[0]
                    
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
        
        progress.update(task, completed=True)
    
    return project_path


def create_claude_commands_symlink(project_path: Path):
    """Create symlink for Claude commands if needed."""
    claude_home = Path.home() / ".claude"
    project_commands = project_path / ".claude" / "commands"
    
    if not project_commands.exists():
        console.print("[yellow]Warning:[/yellow] No .claude/commands directory found in template")
        return
        
    # Check if user has a global .claude/commands
    global_commands = claude_home / "commands"
    
    if global_commands.exists():
        console.print("[cyan]Info:[/cyan] You already have global Claude commands")
        console.print("      To use Specify4 commands in this project, run:")
        console.print(f"      [cyan]cd {project_path.name} && claude[/cyan]")
    else:
        # Offer to create global symlinks
        console.print("\n[cyan]Would you like to install Specify4 commands globally?[/cyan]")
        console.print("This will make /specify, /plan, and /tasks available in all projects.")
        
        if typer.confirm("Install globally?", default=False):
            global_commands.mkdir(parents=True, exist_ok=True)
            for cmd_file in project_commands.glob("*.md"):
                symlink = global_commands / cmd_file.name
                if not symlink.exists():
                    symlink.symlink_to(cmd_file.resolve())
            console.print("[green]✓[/green] Specify4 commands installed globally")


@app.command()
def init(
    project_name: str = typer.Argument(help="Name for your new project directory"),
    ignore_agent_tools: bool = typer.Option(False, "--ignore-agent-tools", help="Skip checks for AI agent tools like Claude Code"),
):
    """
    Initialize a new Specify4 project from the latest template.
    
    This command will:
    1. Check that required tools are installed (git, gh)
    2. Download the latest release from GitHub
    3. Extract the template to a new project directory
    4. Initialize a fresh git repository
    5. Optionally set up Claude commands
    
    Examples:
        specify init my-project
        specify init --ignore-agent-tools my-project
    """
    # Show banner first
    show_banner()
    
    console.print(Panel.fit(
        "[bold cyan]Specify4 Project Setup[/bold cyan]\n"
        f"Creating new project: [green]{project_name}[/green]",
        border_style="cyan"
    ))
    
    # Check required tools
    console.print("\n[bold]Checking required tools...[/bold]")
    
    # Check critical tools (can't proceed without these)
    critical_tools_ok = True
    critical_tools_ok &= check_tool("git", "https://git-scm.com/downloads")
    critical_tools_ok &= check_tool("gh", "https://cli.github.com/")
    
    if not critical_tools_ok:
        console.print("\n[red]Critical tools are missing![/red]")
        console.print("[red]Cannot proceed without git and gh.[/red]")
        raise typer.Exit(1)
    
    # Check optional agent tools
    if not ignore_agent_tools:
        agent_tools_ok = check_claude_code()
        if not agent_tools_ok:
            console.print("\n[yellow]Optional AI agent tools are missing.[/yellow]")
            if not typer.confirm("Continue without AI agent tools?", default=True):
                raise typer.Exit(1)
    
    # Download and set up project
    console.print(f"\n[bold]Setting up project from latest template...[/bold]")
    
    try:
        project_path = download_and_extract_template(project_name)
    except Exception as e:
        console.print(f"[red]Failed to set up project:[/red] {e}")
        raise typer.Exit(1)
    
    # Offer to set up Claude commands
    if not ignore_agent_tools:
        create_claude_commands_symlink(project_path)
    
    # Success!
    console.print("\n" + "─" * 60)
    success_text = Text("✨ Success! ", style="green bold") + Text("Your Specify4 project is ready.", style="white")
    console.print(Align.center(success_text))
    console.print("─" * 60)
    
    console.print(f"\n[bold]Next steps:[/bold]")
    console.print(f"1. [cyan]cd {project_name}[/cyan]")
    console.print(f"2. [cyan]claude[/cyan]  # Start Claude Code")
    console.print(f"3. [cyan]/specify[/cyan] your first feature")
    console.print(f"4. [cyan]/plan[/cyan] the implementation")
    console.print(f"5. [cyan]/tasks[/cyan] to generate tasks")
    
    console.print(f"\n[dim]Your first /plan will replace CLAUDE.md with project-specific context.[/dim]")
    console.print(f"\n[italic bright_yellow]Happy coding with Specify4! 🚀[/italic bright_yellow]\n")


@app.command()
def check():
    """Check that all required tools are installed."""
    show_banner()
    console.print("[bold]Checking Specify4 requirements...[/bold]\n")
    
    all_ok = True
    all_ok &= check_tool("git", "https://git-scm.com/downloads")
    all_ok &= check_claude_code()
    all_ok &= check_tool("gh", "https://cli.github.com/")
    all_ok &= check_tool("uv", "curl -LsSf https://astral.sh/uv/install.sh | sh")
    
    if all_ok:
        console.print("\n[green]✓ All tools installed![/green]")
    else:
        console.print("\n[red]✗ Some tools are missing[/red]")
        raise typer.Exit(1)


def main():
    app()


if __name__ == "__main__":
    main()