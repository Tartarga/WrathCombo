$helperFiles = @(
	"WrathCombo/Combos/PvE/WHM/WHM_Helper.cs",
	"WrathCombo/Combos/PvE/WAR/WAR_Helper.cs",
	"WrathCombo/Combos/PvE/VPR/VPR_Helper.cs",
	"WrathCombo/Combos/PvE/SMN/SMN_Helper.cs",
	"WrathCombo/Combos/PvE/SGE/SGE_Helper.cs",
	"WrathCombo/Combos/PvE/SCH/SCH_Helper.cs",
	"WrathCombo/Combos/PvE/SAM/SAM_Helper.cs",
	"WrathCombo/Combos/PvE/RPR/RPR_Helper.cs",
	"WrathCombo/Combos/PvE/RDM/RDM_Helper.cs",
	"WrathCombo/Combos/PvE/PLD/PLD_Helper.cs",
	"WrathCombo/Combos/PvE/PCT/PCT_Helper.cs",
	"WrathCombo/Combos/PvE/NIN/NIN_Helper.cs",
	"WrathCombo/Combos/PvE/DNC/DNC_Helper.cs",
	"WrathCombo/Combos/PvE/MNK/MNK_Helper.cs",
	"WrathCombo/Combos/PvE/MCH/MCH_Helper.cs",
	"WrathCombo/Combos/PvE/GNB/GNB_Helper.cs",
	"WrathCombo/Combos/PvE/DRK/DRK_Helper.cs",
	"WrathCombo/Combos/PvE/DRG/DRG_Helper.cs",
	"WrathCombo/Combos/PvE/BLM/BLM_Helper.cs"
)

foreach ($file in $helperFiles) {
	$fullPath = "C:\Users\TK\source\repos\WrathCombo\$file"
	if (Test-Path $fullPath) {
		$content = Get-Content $fullPath -Raw

		# Replace patterns: lines that have an action name without () =>
		# This regex finds lines with just an action (no lambda) followed by comma and comment
		$pattern = '(?m)^(\s+)([A-Za-z_][A-Za-z0-9_]*(?:\[[^\]]*\]|\([^)]*\))?(?:\.[A-Za-z_][A-Za-z0-9_]*(?:\[[^\]]*\]|\([^)]*\))?)*)(,\s*//\s*\d+)$'
		$replacement = '$1() => $2$3'

		$newContent = [regex]::Replace($content, $pattern, $replacement)

		# Also fix Items.UseItem calls
		$newContent = $newContent -replace '(?m)^(\s+)(Items\.[^\s,]+)(,\s*//\s*\d+)$', '$1() => $2$3'

		# Also fix Role.* calls
		$newContent = $newContent -replace '(?m)^(\s+)(Role\.[^\s,]+)(,\s*//\s*\d+)$', '$1() => $2$3'

		# Remove duplicate lambdas if any
		$newContent = $newContent -replace '\(\)\s*=>\s*\(\)\s*=>', '() =>'

		Set-Content $fullPath -Value $newContent
		Write-Host "Fixed: $file"
	} else {
		Write-Host "Not found: $file"
	}
}

Write-Host "All files processed"
