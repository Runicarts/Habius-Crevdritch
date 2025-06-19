import os
import shutil
import subprocess
import tkinter as tk
from tkinter import filedialog, messagebox, ttk
import threading
import stat
import time

# Original Bitbucket Repo
DEFAULT_REPO_URL = "https://bitbucket.org/ashwill/larp.git"
DEFAULT_BRANCH_NAME = "master"

# GitHub Repo Details
GITHUB_REPO_URL = "https://github.com/Runicarts/Habius-Crevdritch.git"
GITHUB_BRANCH_NAME = "VRPG-update"

# Dark theme colors
DARK_BG = "#2b2b2b"
DARK_FG = "#ffffff"
DARK_ENTRY_BG = "#404040"
DARK_BUTTON_BG = "#404040"
DARK_BUTTON_HOVER = "#505050"

class GitSyncApp:
    def __init__(self):
        self.window = tk.Tk()
        self.window.title("Unity Assets Git Sync")
        self.window.geometry("600x450") # Increased height for new buttons
        self.window.configure(bg=DARK_BG)

        self.selected_folder = tk.StringVar()
        self.selected_folder.trace('w', self.on_folder_change)
        self.cancel_requested = False
        self.current_operation = None

        # Current active repository and branch
        self.current_repo_url = tk.StringVar(value=DEFAULT_REPO_URL)
        self.current_branch_name = tk.StringVar(value=DEFAULT_BRANCH_NAME)

        self.setup_ui()
        self.update_button_states()
        self.update_repo_display() # Display initial repo


    def setup_ui(self):
        # Main frame
        main_frame = tk.Frame(self.window, bg=DARK_BG, padx=20, pady=20)
        main_frame.pack(expand=True, fill='both')

        # Title
        title_label = tk.Label(main_frame, text="Unity Assets Git Sync",
                              font=('Arial', 14, 'bold'), fg=DARK_FG, bg=DARK_BG)
        title_label.pack(pady=10)

        # Folder selection
        folder_frame = tk.Frame(main_frame, bg=DARK_BG)
        folder_frame.pack(pady=5, fill='x')

        tk.Label(folder_frame, text="Unity Project Assets Folder:", fg=DARK_FG, bg=DARK_BG).pack(side='left')
        self.folder_entry = tk.Entry(folder_frame, textvariable=self.selected_folder, width=40,
                                     bg=DARK_ENTRY_BG, fg=DARK_FG, insertbackground=DARK_FG)
        self.folder_entry.pack(side='left', expand=True, fill='x', padx=5)
        tk.Button(folder_frame, text="Browse", command=self.browse_folder,
                  bg=DARK_BUTTON_BG, fg=DARK_FG, activebackground=DARK_BUTTON_HOVER, activeforeground=DARK_FG).pack(side='left')

        # Current Repository Display
        self.repo_display_label = tk.Label(main_frame, textvariable=self.current_repo_url,
                                            fg=DARK_FG, bg=DARK_BG, wraplength=500, justify='center', font=('Arial', 9))
        self.repo_display_label.pack(pady=5)
        
        # Current Branch Display
        self.branch_display_label = tk.Label(main_frame, textvariable=self.current_branch_name,
                                            fg=DARK_FG, bg=DARK_BG, wraplength=500, justify='center', font=('Arial', 9))
        self.branch_display_label.pack(pady=0)


        # Status Label
        self.status_label = tk.Label(main_frame, text="Please select your Unity project Assets Folder.",
                                      fg=DARK_FG, bg=DARK_BG, wraplength=500, justify='center')
        self.status_label.pack(pady=10)

        # Progress bar
        self.progress_bar = ttk.Progressbar(main_frame, orient="horizontal", length=300, mode="indeterminate", style="TProgressbar")
        self.progress_bar.pack(pady=5)
        self.progress_bar.pack_forget() # Hide initially

        # Style for progress bar
        style = ttk.Style()
        style.theme_use('default')
        style.configure("TProgressbar", foreground="#007acc", background="#007acc", troughcolor=DARK_ENTRY_BG)

        # Buttons frame
        buttons_frame = tk.Frame(main_frame, bg=DARK_BG)
        buttons_frame.pack(pady=10)

        self.upload_button = tk.Button(buttons_frame, text="Upload to Current Repo", command=self.upload,
                                       bg=DARK_BUTTON_BG, fg=DARK_FG, activebackground=DARK_BUTTON_HOVER, activeforeground=DARK_FG)
        self.upload_button.pack(side='left', padx=10)

        self.download_button = tk.Button(buttons_frame, text="Download from Current Repo", command=self.download,
                                         bg=DARK_BUTTON_BG, fg=DARK_FG, activebackground=DARK_BUTTON_HOVER, activeforeground=DARK_FG)
        self.download_button.pack(side='left', padx=10)

        self.cancel_button = tk.Button(buttons_frame, text="Cancel", command=self.cancel_operation,
                                       bg=DARK_BUTTON_BG, fg=DARK_FG, activebackground=DARK_BUTTON_HOVER, activeforeground=DARK_FG)
        self.cancel_button.pack(side='left', padx=10)
        self.cancel_button.config(state='disabled')

        # New Reset Git Repo button
        self.reset_git_button = tk.Button(main_frame, text="Reset Git Repository", command=self.reset_git_repo_ui,
                                          bg="#cc0000", fg=DARK_FG, activebackground="#ff3333", activeforeground=DARK_FG)
        self.reset_git_button.pack(pady=10)
        self.reset_git_button.config(state='disabled')

        # New Repo Selection Buttons Frame
        repo_selection_frame = tk.Frame(main_frame, bg=DARK_BG)
        repo_selection_frame.pack(pady=10)

        self.bitbucket_button = tk.Button(repo_selection_frame, text="Set Bitbucket Repo",
                                           command=self.set_bitbucket_repo,
                                           bg=DARK_BUTTON_BG, fg=DARK_FG, activebackground=DARK_BUTTON_HOVER, activeforeground=DARK_FG)
        self.bitbucket_button.pack(side='left', padx=5)

        self.github_button = tk.Button(repo_selection_frame, text="Set GitHub Repo",
                                        command=self.set_github_repo,
                                        bg=DARK_BUTTON_BG, fg=DARK_FG, activebackground=DARK_BUTTON_HOVER, activeforeground=DARK_FG)
        self.github_button.pack(side='left', padx=5)

    def update_repo_display(self):
        self.repo_display_label.config(text=f"Current Repository: {self.current_repo_url.get()}")
        self.branch_display_label.config(text=f"Current Branch: {self.current_branch_name.get()}")


    def set_bitbucket_repo(self):
        self.current_repo_url.set(DEFAULT_REPO_URL)
        self.current_branch_name.set(DEFAULT_BRANCH_NAME)
        self.update_repo_display()
        self.status_label.config(text="Repository set to Bitbucket (larp). Please proceed.")

    def set_github_repo(self):
        self.current_repo_url.set(GITHUB_REPO_URL)
        self.current_branch_name.set(GITHUB_BRANCH_NAME)
        self.update_repo_display()
        self.status_label.config(text="Repository set to GitHub (Habius-Crevdritch). Please proceed.")


    def browse_folder(self):
        folder_path = filedialog.askdirectory()
        if folder_path:
            self.selected_folder.set(folder_path)

    def on_folder_change(self, *args):
        self.update_button_states()
        folder = self.selected_folder.get()
        if folder:
            if os.path.exists(os.path.join(folder, ".git")):
                self.status_label.config(text=f"Ready. Git repository detected in {folder}.")
            else:
                self.status_label.config(text=f"No Git repository found in {folder}. It will be initialized.")
        else:
            self.status_label.config(text="Please select your Unity project root folder.")

    def update_button_states(self):
        has_folder = bool(self.selected_folder.get())
        is_operating = self.current_operation is not None

        self.upload_button.config(state='disabled' if not has_folder or is_operating else 'normal')
        self.download_button.config(state='disabled' if not has_folder or is_operating else 'normal')
        self.reset_git_button.config(state='disabled' if not has_folder or is_operating else 'normal')
        self.cancel_button.config(state='normal' if is_operating else 'disabled')
        self.folder_entry.config(state='disabled' if is_operating else 'normal')
        self.bitbucket_button.config(state='disabled' if is_operating else 'normal')
        self.github_button.config(state='disabled' if is_operating else 'normal')


    def show_progress(self, message):
        self.status_label.config(text=message)
        self.progress_bar.pack(pady=5)
        self.progress_bar.start(10) # Start animation
        self.update_button_states() # Disable buttons during operation

    def hide_progress(self):
        self.progress_bar.stop()
        self.progress_bar.pack_forget()
        self.update_button_states() # Re-enable buttons

    def run_git_command(self, command, cwd):
        print(f"\nRunning command: {' '.join(command)} in {cwd}") # Debug print
        try:
            process = subprocess.run(command, cwd=cwd, capture_output=True, text=True)
            stdout = process.stdout.strip()
            stderr = process.stderr.strip()

            print(f"STDOUT:\n{stdout}") # Debug print
            print(f"STDERR:\n{stderr}") # Debug print

            if process.returncode != 0:
                # Return both stdout and stderr for better error reporting
                return f"Error (Exit Code {process.returncode}):\nSTDOUT: {stdout}\nSTDERR: {stderr}"
            else:
                return stdout
        except FileNotFoundError:
            return "Error: Git command not found. Please ensure Git is installed and in your PATH."
        except Exception as e:
            return f"Error: An unexpected Python error occurred: {str(e)}"


    def setup_git_repo(self, root_folder):
        git_dir = os.path.join(root_folder, ".git")
        repo_url_to_use = self.current_repo_url.get()

        if not os.path.exists(git_dir):
            self.window.after(0, lambda: self.status_label.config(text="Initializing new Git repository..."))
            init_result = self.run_git_command(["git", "init"], root_folder)
            if "Error:" in init_result:
                self.window.after(0, lambda: messagebox.showerror("Git Init Error", init_result))
                return False

            self.window.after(0, lambda: self.status_label.config(text="Adding remote origin..."))
            remote_result = self.run_git_command(["git", "remote", "add", "origin", repo_url_to_use], root_folder)
            if "Error:" in remote_result and "already exists" not in remote_result:
                self.window.after(0, lambda: messagebox.showerror("Git Remote Error", remote_result))
                return False
            return True
        else:
            self.window.after(0, lambda: self.status_label.config(text="Git repository already exists."))
            # Ensure the remote URL is correct if repo exists
            remote_check = self.run_git_command(["git", "remote", "get-url", "origin"], root_folder)
            if "Error:" in remote_check: # Handle error if remote doesn't exist for some reason
                 if messagebox.askyesno("Remote Origin Error", "Could not check remote origin URL. Do you want to try adding/setting it?"):
                     self.run_git_command(["git", "remote", "set-url", "origin", repo_url_to_use], root_folder) # Try setting it
                     self.window.after(0, lambda: self.status_label.config(text="Remote origin URL attempted to be set."))
                 return True # Continue, as error might be temporary
            elif repo_url_to_use not in remote_check:
                if messagebox.askyesno("Remote URL Mismatch",
                                       f"Current remote origin URL is '{remote_check}' but expected '{repo_url_to_use}'.\n"
                                       "Do you want to update it?"):
                    self.run_git_command(["git", "remote", "set-url", "origin", repo_url_to_use], root_folder)
                    self.window.after(0, lambda: self.status_label.config(text="Remote origin URL updated."))
            return True


    def ensure_initial_commit(self, root_folder):
        # Check if there are any commits on the current branch
        log_check = self.run_git_command(["git", "rev-parse", "--verify", "HEAD"], root_folder)

        # Check for specific error messages indicating no commits
        if "Error:" in log_check and ("unknown revision or path not in the working tree" in log_check or "ambiguous argument 'HEAD'" in log_check):
            self.window.after(0, lambda: self.status_label.config(text="No initial commit found. Creating one..."))
            
            # Add all files in the current working directory to staging
            add_result = self.run_git_command(["git", "add", "."], root_folder)
            if "Error:" in add_result and "nothing specified" not in add_result:
                self.window.after(0, lambda: messagebox.showerror("Git Add Error (Initial Commit)", add_result))
                return False

            # Check if there's anything staged to commit
            status_output = self.run_git_command(["git", "status", "--porcelain"], root_folder)
            if not status_output or "Error:" in status_output:
                if "Error:" in status_output: # If status itself returned an error
                    self.window.after(0, lambda: messagebox.showerror("Git Status Check Error", status_output))
                    return False
                self.window.after(0, lambda: self.status_label.config(text="No changes found for initial commit."))
                return True # Nothing to commit is not a failure for initial commit
            else:
                commit_result = self.run_git_command(["git", "commit", "-m", "Initial commit from Pushit.py"], root_folder)
                if "Error:" in commit_result:
                    self.window.after(0, lambda: messagebox.showerror("Git Commit Error", commit_result))
                    return False
                self.window.after(0, lambda: self.status_label.config(text="Initial commit created."))
                return True
        elif "Error:" in log_check: # Other types of errors from rev-parse
            self.window.after(0, lambda: messagebox.showerror("Git Rev-Parse Error", log_check))
            return False
        return True # Initial commit already exists


    def reset_git_repo_ui(self):
        root_folder = self.selected_folder.get()
        if not root_folder:
            messagebox.showerror("Error", "Please select a Unity project root folder first.")
            return

        if messagebox.askyesno("Confirm Git Repository Reset",
                               "This will delete the .git folder in your selected project root and re-initialize "
                               "the Git repository. This action is irreversible and will erase your local Git history. "
                               "Do you want to proceed?"):
            self.current_operation = 'reset'
            self.show_progress("Resetting Git repository...")
            threading.Thread(target=self._reset_git_repo_worker, args=(root_folder,)).start()

    def _reset_git_repo_worker(self, root_folder):
        try:
            git_path = os.path.join(root_folder, ".git")

            if os.path.exists(git_path):
                # On Windows, shutil.rmtree can fail if files are read-only.
                # Need to set permissions to writable first.
                for dirpath, dirnames, filenames in os.walk(git_path):
                    for fname in filenames:
                        fpath = os.path.join(dirpath, fname)
                        try:
                            os.chmod(fpath, stat.S_IWRITE)
                        except OSError as e:
                            print(f"Warning: Could not change permissions for {fpath}: {e}")
                shutil.rmtree(git_path)
                self.window.after(0, lambda: self.status_label.config(text="Deleted existing .git folder."))
            else:
                self.window.after(0, lambda: self.status_label.config(text="No .git folder found to delete. Proceeding with initialization."))

            if not self.setup_git_repo(root_folder):
                self.window.after(0, lambda: messagebox.showerror("Git Reset Error", "Failed to re-initialize Git repository."))
                return

            if not self.ensure_initial_commit(root_folder):
                 self.window.after(0, lambda: messagebox.showerror("Git Reset Error", "Failed to create initial commit after reset."))
                 return

            self.window.after(0, lambda: messagebox.showinfo("Git Reset Complete",
                                                            "Git repository has been successfully reset and re-initialized!"))
        except Exception as e:
            self.window.after(0, lambda: messagebox.showerror("Unexpected Error",
                f"An unexpected error occurred during Git reset: {str(e)}"))
        finally:
            self.current_operation = None
            self.window.after(0, self.hide_progress)


    def upload_worker(self):
        root_folder = self.selected_folder.get()
        if not root_folder:
            self.window.after(0, lambda: messagebox.showerror("Upload Error", "No folder selected."))
            return

        repo_url_to_use = self.current_repo_url.get()
        branch_name_to_use = self.current_branch_name.get()

        try:
            self.window.after(0, lambda: self.show_progress("Setting up Git repository..."))
            if not self.setup_git_repo(root_folder):
                return # setup_git_repo will show its own error

            self.window.after(0, lambda: self.show_progress("Ensuring initial commit..."))
            if not self.ensure_initial_commit(root_folder):
                return # ensure_initial_commit will show its own error

            self.window.after(0, lambda: self.show_progress("Adding all changes to Git..."))
            # Use 'git add .' to ensure all untracked and modified files are staged
            add_result = self.run_git_command(["git", "add", "."], root_folder)
            if "Error:" in add_result and "nothing specified" not in add_result:
                self.window.after(0, lambda: messagebox.showerror("Git Add Error", add_result))
                return

            # Check for staged changes before committing
            status_output = self.run_git_command(["git", "status", "--porcelain"], root_folder)
            if not status_output or "Error:" in status_output:
                if "Error:" in status_output: # If status itself returned an error
                    self.window.after(0, lambda: messagebox.showerror("Git Status Check Error", status_output))
                    return
                self.window.after(0, lambda: self.status_label.config(text="No changes to commit. Local repository is up to date."))
                self.window.after(0, lambda: messagebox.showinfo("Upload Complete", "No changes to upload. Local repository is already up to date."))
                return

            self.window.after(0, lambda: self.show_progress("Committing changes..."))
            commit_message = f"Automatic upload from Pushit.py on {time.strftime('%Y-%m-%d %H:%M:%S')}"
            commit_result = self.run_git_command(["git", "commit", "-m", commit_message], root_folder)
            if "Error:" in commit_result and "nothing to commit" not in commit_result.lower(): # Only error if it's a real commit error
                self.window.after(0, lambda: messagebox.showerror("Git Commit Error", commit_result))
                return
            elif "nothing to commit" in commit_result.lower():
                 self.window.after(0, lambda: self.status_label.config(text="No new changes to commit."))
                 self.window.after(0, lambda: messagebox.showinfo("Upload Complete", "No new changes to upload."))
                 return # Exit early if nothing to commit

            self.window.after(0, lambda: self.show_progress(f"Pushing to {repo_url_to_use} on branch {branch_name_to_use}..."))
            # Force push is used as per user's original request, use with caution.
            # If you want to avoid force push, remove '-f' and handle merge conflicts manually.
            push_result = self.run_git_command(["git", "push", "-f", "origin", branch_name_to_use], root_folder)
            if "Error:" in push_result:
                self.window.after(0, lambda: messagebox.showerror("Git Push Error", push_result))
                return

            self.window.after(0, lambda: messagebox.showinfo("Upload Complete",
                                                            "Successfully uploaded!"))

        except Exception as e:
            self.window.after(0, lambda: messagebox.showerror("Unexpected Error",
                f"An unexpected error occurred during upload: {str(e)}"))
        finally:
            self.current_operation = None
            self.window.after(0, self.hide_progress)

    def download_worker(self):
        root_folder = self.selected_folder.get()
        if not root_folder:
            self.window.after(0, lambda: messagebox.showerror("Download Error", "No folder selected."))
            return

        repo_url_to_use = self.current_repo_url.get()
        branch_name_to_use = self.current_branch_name.get()

        try:
            self.window.after(0, lambda: self.show_progress("Setting up Git repository for download..."))
            if not self.setup_git_repo(root_folder):
                return # setup_git_repo will show its own error

            # Ensure the Assets folder exists in the project root to pull into
            # This check is more robust if the user selected the actual project root.
            # If user selected Assets folder, this will create Assets/Assets which is fine.
            # However, `git pull` generally handles creating directories as needed for fetched files.
            if not os.path.exists(os.path.join(root_folder, "Assets")) and not os.path.exists(os.path.join(root_folder, "ProjectSettings")):
                # If neither Assets nor ProjectSettings exist directly, it's possible the user picked an empty folder
                # or a very high level folder. Creating 'Assets' is a sensible default.
                os.makedirs(os.path.join(root_folder, "Assets"), exist_ok=True)


            self.window.after(0, lambda: self.show_progress(f"Downloading from {repo_url_to_use} on branch {branch_name_to_use}..."))
            pull_result = self.run_git_command(["git", "pull", "origin", branch_name_to_use], root_folder)
            if "Error:" in pull_result:
                self.window.after(0, lambda: messagebox.showerror("Git Pull Error", pull_result))
                return

            self.window.after(0, lambda: messagebox.showinfo("Download Complete",
                                                            "Successfully downloaded!\n\n" +\
                                                            "Your local files have been updated."))

        except Exception as e:
            self.window.after(0, lambda: messagebox.showerror("Unexpected Error",
                f"An unexpected error occurred during download: {str(e)}"))
        finally:
            self.current_operation = None
            self.window.after(0, self.hide_progress)

    def upload(self):
        self.cancel_requested = False
        self.current_operation = 'upload'
        thread = threading.Thread(target=self.upload_worker)
        thread.daemon = True
        thread.start()

    def download(self):
        self.cancel_requested = False
        self.current_operation = 'download'
        thread = threading.Thread(target=self.download_worker)
        thread.daemon = True
        thread.start()

    def cancel_operation(self):
        self.cancel_requested = True
        self.window.after(0, lambda: self.status_label.config(text="Cancelling current operation..."))

    def run(self):
        self.window.mainloop()

# Run the application
if __name__ == "__main__":
    app = GitSyncApp()
    app.run()