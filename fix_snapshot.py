filepath = r'C:\Users\LAP LINK\source\repos\FinalLabSystem\FinalLabSystem\Migrations\FinalLabDbContextModelSnapshot.cs'
with open(filepath, 'r', encoding='utf-16') as f:
    content = f.read()

replacements = [
    # 4. BackupOutputFolder
    ('.HasColumnType("nvarchar(500)")\r\n                        .HasColumnName("backup_output_folder")',
     '.HasColumnType("nvarchar(max)")\r\n                        .HasColumnName("backup_output_folder")'),
    # 5. ReportLogoWidth
    ('.HasColumnType("decimal(5,2)")\r\n                        .HasColumnName("ReportLogoWidth")',
     '.HasColumnType("decimal(18,2)")\r\n                        .HasColumnName("ReportLogoWidth")'),
    # 6. ReportLogoHeight
    ('.HasColumnType("decimal(5,2)")\r\n                        .HasColumnName("ReportLogoHeight")',
     '.HasColumnType("decimal(18,2)")\r\n                        .HasColumnName("ReportLogoHeight")'),
    # 7. ReportMarginTop
    ('.HasColumnType("decimal(5,2)")\r\n                        .HasColumnName("ReportMarginTop")',
     '.HasColumnType("decimal(18,2)")\r\n                        .HasDefaultValue(2m)\r\n                        .HasColumnName("ReportMarginTop")'),
    # 8. ReportMarginBottom
    ('.HasColumnType("decimal(5,2)")\r\n                        .HasColumnName("ReportMarginBottom")',
     '.HasColumnType("decimal(18,2)")\r\n                        .HasDefaultValue(2m)\r\n                        .HasColumnName("ReportMarginBottom")'),
    # 9. ReportMarginLeft
    ('.HasColumnType("decimal(5,2)")\r\n                        .HasColumnName("ReportMarginLeft")',
     '.HasColumnType("decimal(18,2)")\r\n                        .HasDefaultValue(2m)\r\n                        .HasColumnName("ReportMarginLeft")'),
    # 10. ReportMarginRight
    ('.HasColumnType("decimal(5,2)")\r\n                        .HasColumnName("ReportMarginRight")',
     '.HasColumnType("decimal(18,2)")\r\n                        .HasDefaultValue(2m)\r\n                        .HasColumnName("ReportMarginRight")'),
    # 11. ReportShowHeader
    ('.HasColumnType("bit")\r\n                        .HasColumnName("ReportShowHeader")',
     '.HasColumnType("bit")\r\n                        .HasDefaultValue(true)\r\n                        .HasColumnName("ReportShowHeader")'),
    # 12. ReportShowFooter
    ('.HasColumnType("bit")\r\n                        .HasColumnName("ReportShowFooter")',
     '.HasColumnType("bit")\r\n                        .HasDefaultValue(true)\r\n                        .HasColumnName("ReportShowFooter")'),
    # 13. ReportShowStamp
    ('.HasColumnType("bit")\r\n                        .HasColumnName("ReportShowStamp")',
     '.HasColumnType("bit")\r\n                        .HasDefaultValue(false)\r\n                        .HasColumnName("ReportShowStamp")'),
    # 14. ReportFontSize
    ('.HasColumnType("float")\r\n                        .HasColumnName("ReportFontSize")',
     '.HasColumnType("float")\r\n                        .HasDefaultValue(12.0)\r\n                        .HasColumnName("ReportFontSize")'),
    # 15. ReportHeaderFontSize
    ('.HasColumnType("float")\r\n                        .HasColumnName("ReportHeaderFontSize")',
     '.HasColumnType("float")\r\n                        .HasDefaultValue(16.0)\r\n                        .HasColumnName("ReportHeaderFontSize")'),
    # 16. ReportFooterFontSize
    ('.HasColumnType("float")\r\n                        .HasColumnName("ReportFooterFontSize")',
     '.HasColumnType("float")\r\n                        .HasDefaultValue(10.0)\r\n                        .HasColumnName("ReportFooterFontSize")'),
]

applied = 0
for old, new in replacements:
    if old in content:
        content = content.replace(old, new)
        applied += 1
        print(f"  OK: Applied replacement #{applied}")
    else:
        print(f"  SKIP: Pattern not found for replacement")

with open(filepath, 'w', encoding='utf-16') as f:
    f.write(content)

print(f"\nTotal applied: {applied}/{len(replacements)}")
