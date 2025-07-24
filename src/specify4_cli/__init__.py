#!/usr/bin/env python3
# /// script
# requires-python = ">=3.11"
# dependencies = [
#     "typer",
#     "rich",
#     "httpx",
#     "platformdirs",
# ]
# ///
"""
Specify CLI - Setup tool for Specify4 projects

Usage:
    uvx specify-cli.py clone <repo-url> <local-name>
    
Or install globally:
    uv tool install --from specify-cli.py specify-cli
    specify clone <repo-url> <local-name>
"""

import os
import subprocess
import sys
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


def run_command(cmd: list[str], check: bool = True, capture: bool = False) -> Optional[str]:
    """Run a shell command and optionally capture output."""
    try:
        if capture:
            result = subprocess.run(cmd, check=check, capture_output=True, text=True)
            return result.stdout.strip()
        else:
            subprocess.run(cmd, check=check)
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
    try:
        run_command(["which", tool], capture=True)
        return True
    except subprocess.CalledProcessError:
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


def setup_git_repo(repo_url: str, local_name: str) -> Path:
    """Clone the repository and set it up."""
    local_path = Path(local_name).resolve()
    
    if local_path.exists():
        console.print(f"[red]Error:[/red] Directory '{local_name}' already exists")
        raise typer.Exit(1)
    
    with Progress(
        SpinnerColumn(),
        TextColumn("[progress.description]{task.description}"),
        console=console,
    ) as progress:
        # Clone repository
        task = progress.add_task("Cloning repository...", total=None)
        run_command(["git", "clone", repo_url, str(local_path)])
        progress.update(task, completed=True)
        
        # Clean up for fresh start
        task = progress.add_task("Cleaning up template files...", total=None)
        os.chdir(local_path)
        
        # Remove git history
        run_command(["rm", "-rf", ".git"])
        
        # Note: We no longer need to remove specs/ as it's not included in the template
        
        # Initialize fresh git repo
        run_command(["git", "init"])
        run_command(["git", "add", "."])
        run_command(["git", "commit", "-m", "Initial commit from Specify4 template"])
        
        progress.update(task, completed=True)
    
    return local_path


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
def clone(
    repo_url: str = typer.Argument(help="URL of the Specify4 repository (or your fork)"),
    local_name: str = typer.Argument(help="Name for your local project directory"),
    check_tools: bool = typer.Option(True, "--check-tools/--no-check-tools", help="Check for required tools"),
):
    """
    Clone a Specify4 template repository and set up your project.
    
    This command will:
    1. Check that required tools are installed (claude, git, gh)
    2. Clone the repository
    3. Remove git history
    4. Initialize a fresh git repository
    5. Optionally set up Claude commands
    
    Examples:
        specify clone https://github.com/jflam/specify4.git my-project
        specify clone git@github.com:jflam/specify4.git my-project
    """
    # Show banner first
    show_banner()
    
    console.print(Panel.fit(
        "[bold cyan]Specify4 Project Setup[/bold cyan]\n"
        f"Creating new project: [green]{local_name}[/green]",
        border_style="cyan"
    ))
    
    # Check required tools
    if check_tools:
        console.print("\n[bold]Checking required tools...[/bold]")
        
        tools_ok = True
        tools_ok &= check_tool("git", "https://git-scm.com/downloads")
        tools_ok &= check_claude_code()
        tools_ok &= check_tool("gh", "https://cli.github.com/")
        
        if not tools_ok:
            console.print("\n[red]Missing required tools![/red]")
            if not typer.confirm("Continue anyway?", default=False):
                raise typer.Exit(1)
    
    # Clone and set up repository
    console.print(f"\n[bold]Setting up project from {repo_url}...[/bold]")
    
    try:
        project_path = setup_git_repo(repo_url, local_name)
    except subprocess.CalledProcessError:
        console.print("[red]Failed to set up repository[/red]")
        raise typer.Exit(1)
    
    # Offer to set up Claude commands
    create_claude_commands_symlink(project_path)
    
    # Success!
    console.print("\n" + "─" * 60)
    success_text = Text("✨ Success! ", style="green bold") + Text("Your Specify4 project is ready.", style="white")
    console.print(Align.center(success_text))
    console.print("─" * 60)
    
    console.print(f"\n[bold]Next steps:[/bold]")
    console.print(f"1. [cyan]cd {local_name}[/cyan]")
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