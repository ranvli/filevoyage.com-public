# GitHub Actions on Acer

Updated: 2026-10-09.

Both existing workflows retain their triggers and Azure deployment target filevoyage. Trusted jobs use the repository runner Acer-filevoyage.com-public-Linux with labels self-hosted, Linux, X64, acer-linux. Existing authentication references and application code are unchanged.

The migration commit uses [skip ci] because this repository has been dormant and its legacy production target has not been validated. This configures Acer for future runs; it does not prove the old deployment still works. The active filevoyage.com repository is tracked separately.

Ubuntu-24.04 runs in WSL2 with its VHD under D:\BuildFarm\wsl\Ubuntu. Runner, tool, workspace and cache files under /home/buildfarm/farm are inside that D: VHD. Publish output stays in the job workspace, outside the shared SDK directory.

The AlienCatBuildFarm task starts the supervisor at Randall's Windows logon. Keep Acer awake, connected and logged in. Linux jobs share a serialized queue. External fork pull requests use GitHub-hosted execution where applicable.

GitHub remains the trigger, queue, log and artifact service. Artifact storage and unrelated subscriptions can still incur charges. No new Azure resources are added.

Verification and recovery: https://docs.google.com/spreadsheets/d/1XT4srZrZ6zmDQolfMQC0KRYGr8Xvjl9wDEzqr8kEcbU/edit . To restore hosted execution, revert the migration commit or restore the original runs-on and publish path.
