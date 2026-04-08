using System.Diagnostics;

namespace AdoToolkit.Tests.Contract;

public sealed class HomebrewFormulaContractTests
{
    [Fact]
    public async Task Package_script_generates_formula_with_expected_install_contract()
    {
        var repoRoot = "/Users/john/Source/repos/xelseor/ado-toolkit";
        var distDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(distDir, "publish", "macos-arm64"));
        Directory.CreateDirectory(Path.Combine(distDir, "publish", "macos-x64"));

        await File.WriteAllTextAsync(Path.Combine(distDir, "publish", "macos-arm64", "ado"), "#!/bin/sh\necho arm64\n");
        await File.WriteAllTextAsync(Path.Combine(distDir, "publish", "macos-x64", "ado"), "#!/bin/sh\necho x64\n");

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/zsh",
                ArgumentList =
                {
                    "-lc",
                    $"DIST_DIR='{distDir}' /Users/john/Source/repos/xelseor/ado-toolkit/scripts/package-homebrew.sh --skip-publish"
                },
                WorkingDirectory = repoRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        process.Start();
        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        Assert.True(process.ExitCode == 0, $"Packaging script failed.\nSTDOUT:\n{stdout}\nSTDERR:\n{stderr}");

        var formula = await File.ReadAllTextAsync(Path.Combine(distDir, "ado-toolkit.rb"));
        Assert.Contains("class AdoToolkit < Formula", formula);
        Assert.Contains("bin.install \"ado\"", formula);
        Assert.Contains("assert_match \"ADO Toolkit\", shell_output(\"#{bin}/ado --help\")", formula);
        Assert.Contains("ado-toolkit-0.1.0-macos-arm64.tar.gz", formula);
        Assert.Contains("ado-toolkit-0.1.0-macos-x64.tar.gz", formula);
        Assert.DoesNotContain("__VERSION__", formula);
        Assert.DoesNotContain("__ARM64_SHA__", formula);
        Assert.DoesNotContain("__X64_SHA__", formula);
    }
}
