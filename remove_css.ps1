Set-Location 'C:\Users\Eric\Desktop\客戶\JSIS'

# Helper: remove lines startLine..endLine (1-indexed, inclusive) from file
function RemoveLines($filePath, $startLine, $endLine) {
    $lines = [System.IO.File]::ReadAllLines($filePath)
    $new = @()
    if ($startLine -gt 1) { $new += $lines[0..($startLine-2)] }
    if ($endLine -lt $lines.Length) { $new += $lines[$endLine..($lines.Length-1)] }
    [System.IO.File]::WriteAllLines($filePath, $new, [System.Text.UTF8Encoding]::new($false))
    Write-Host "OK $filePath : removed $($endLine-$startLine+1) lines, new total=$($new.Length)"
}

# === 1. Add .inq-input-wide to _InquiryTemplate.cshtml ===
$tpl = 'Pages\Shared\_InquiryTemplate.cshtml'
$tplContent = [System.IO.File]::ReadAllText($tpl, [System.Text.Encoding]::UTF8)
if ($tplContent -notmatch '\.inq-input-wide') {
    # Insert after .inq-input block
    $tplContent = $tplContent -replace '(\.inq-input \{[^}]+\})', '$1
  .inq-input-wide {
    width: 240px;
  }
  .inq-input-short {
    width: 80px;
  }'
    [System.IO.File]::WriteAllText($tpl, $tplContent, [System.Text.UTF8Encoding]::new($false))
    Write-Host "Added .inq-input-wide to template"
} else {
    Write-Host "Template already has .inq-input-wide"
}

# === 2. Complete CSS block removal ===
# APRdAdvanceInq: style at lines 12-193 (all duplicate)
RemoveLines 'Pages\APR\APRdAdvanceInq.cshtml' 12 193

# SBP00035: style at lines 151-185 (all duplicate, has .inq-input-wide now in template)
RemoveLines 'Pages\SBP\SBP00035.cshtml' 151 185

# FME00014: style at lines 158-290 (all duplicate, has .inq-input-wide now in template)
RemoveLines 'Pages\FME\FME00014.cshtml' 158 290

# === 3. Partial CSS removal - keep <style> tag at line X, remove lines X+1 to Y ===
# APRdPRInq: remove duplicate CSS lines 13-195, keep <style> at 12 and unique .inq-split-* at 196+
RemoveLines 'Pages\APR\APRdPRInq.cshtml' 13 195

# SPO00003: remove duplicate CSS lines 299-468, keep <style> at 298 and .customer-multi-modal at 469+
RemoveLines 'Pages\SPO\SPO00003.cshtml' 299 468

# MPH00016: remove duplicate CSS lines 310-469, keep <style> at 309 and sort-override+.outinq at 470+
RemoveLines 'Pages\MPH\MPH00016.cshtml' 310 469

# FPE00020: remove duplicate CSS lines 83-119, keep <style> at 82 and .inq-input-wide+ at 120+
RemoveLines 'Pages\FPE\FPE00020.cshtml' 83 119

# === 4. Middle CSS removal - keep busy-pending at start, remove duplicate in middle ===
# MPH00018: keep 291-323 (busy-pending), remove 324-484 (duplicate table CSS), keep 485-505 (.inq-to etc.)
RemoveLines 'Pages\MPH\MPH00018.cshtml' 324 484

# MPH00019: same structure as MPH00018
RemoveLines 'Pages\MPH\MPH00019.cshtml' 324 484

# SPO00014: same structure as MPH00018
RemoveLines 'Pages\SPO\SPO00014.cshtml' 324 484

# SPO00015: same structure as MPH00018
RemoveLines 'Pages\SPO\SPO00015.cshtml' 324 484

Write-Host "`nAll CSS cleanup complete!"