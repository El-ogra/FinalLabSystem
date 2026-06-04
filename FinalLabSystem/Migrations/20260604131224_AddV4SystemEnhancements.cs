using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddV4SystemEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AntibioticCatalog",
                columns: table => new
                {
                    antibiotic_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    antibiotic_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    antibiotic_class = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_safe_pregnancy = table.Column<bool>(type: "bit", nullable: false),
                    is_safe_children = table.Column<bool>(type: "bit", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AntibioticCatalog", x => x.antibiotic_id);
                });

            // ExternalLab table - pre-existing V3.0 table, not created in this migration

            // Permission table - pre-existing V3.0 table, not created in this migration

            // PriceScheme table - pre-existing V3.0 table, not created in this migration

            // Staff table - pre-existing V3.0 table, not created in this migration

            // TestCategory table - pre-existing V3.0 table, not created in this migration

            migrationBuilder.CreateTable(
                name: "WorkShift",
                columns: table => new
                {
                    shift_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    shift_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    clock_in_time = table.Column<TimeSpan>(type: "time", nullable: false),
                    clock_out_time = table.Column<TimeSpan>(type: "time", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkShift", x => x.shift_id);
                });

            // Company table - pre-existing V3.0 table, not created in this migration

            // ReferralSource table - pre-existing V3.0 table, not created in this migration

            // AuditLog table - pre-existing V3.0 table, not created in this migration

            migrationBuilder.CreateTable(
                name: "ExternalShipment",
                columns: table => new
                {
                    shipment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    external_lab_id = table.Column<int>(type: "int", nullable: false),
                    shipment_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: true),
                    tracking_number = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalShipment", x => x.shipment_id);
                    table.ForeignKey(
                        name: "FK_ExternalShipment_ExternalLab",
                        column: x => x.external_lab_id,
                        principalTable: "ExternalLab",
                        principalColumn: "external_lab_id");
                    table.ForeignKey(
                        name: "FK_ExternalShipment_Staff",
                        column: x => x.created_by,
                        principalTable: "Staff",
                        principalColumn: "staff_id");
                });

            // LabSettings table - pre-existing V3.0 table, not created in this migration

            // Patient table - pre-existing V3.0 table, not created in this migration

            // StaffPermission table - pre-existing V3.0 table, not created in this migration

            migrationBuilder.CreateTable(
                name: "TestProfile",
                columns: table => new
                {
                    profile_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    profile_name_ar = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    profile_name_en = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestProfile", x => x.profile_id);
                    table.ForeignKey(
                        name: "FK_TestProfile_Staff",
                        column: x => x.created_by,
                        principalTable: "Staff",
                        principalColumn: "staff_id");
                });

            // TestGroup table - pre-existing V3.0 table, not created in this migration

            migrationBuilder.CreateTable(
                name: "Attendance",
                columns: table => new
                {
                    attendance_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    staff_id = table.Column<int>(type: "int", nullable: false),
                    shift_id = table.Column<int>(type: "int", nullable: true),
                    clock_in = table.Column<DateTime>(type: "datetime2", nullable: false),
                    clock_out = table.Column<DateTime>(type: "datetime2", nullable: true),
                    late_minutes = table.Column<int>(type: "int", nullable: true),
                    absence_status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    attendance_date = table.Column<DateOnly>(type: "date", nullable: false),
                    StaffId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendance", x => x.attendance_id);
                    table.ForeignKey(
                        name: "FK_Attendance_Staff",
                        column: x => x.staff_id,
                        principalTable: "Staff",
                        principalColumn: "staff_id");
                    table.ForeignKey(
                        name: "FK_Attendance_Staff_StaffId1",
                        column: x => x.StaffId1,
                        principalTable: "Staff",
                        principalColumn: "staff_id");
                    table.ForeignKey(
                        name: "FK_Attendance_WorkShift",
                        column: x => x.shift_id,
                        principalTable: "WorkShift",
                        principalColumn: "shift_id");
                });

            migrationBuilder.CreateTable(
                name: "ContractInvoice",
                columns: table => new
                {
                    contract_invoice_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    company_id = table.Column<int>(type: "int", nullable: false),
                    invoice_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    period_start = table.Column<DateOnly>(type: "date", nullable: false),
                    period_end = table.Column<DateOnly>(type: "date", nullable: false),
                    total_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    paid_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractInvoice", x => x.contract_invoice_id);
                    table.ForeignKey(
                        name: "FK_ContractInvoice_Company",
                        column: x => x.company_id,
                        principalTable: "Company",
                        principalColumn: "company_id");
                    table.ForeignKey(
                        name: "FK_ContractInvoice_Staff",
                        column: x => x.created_by,
                        principalTable: "Staff",
                        principalColumn: "staff_id");
                });

            migrationBuilder.CreateTable(
                name: "PatientMedicalHistory",
                columns: table => new
                {
                    medical_history_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    patient_id = table.Column<int>(type: "int", nullable: false),
                    history_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    start_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    end_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientMedicalHistory", x => x.medical_history_id);
                    table.ForeignKey(
                        name: "FK_PatientMedicalHistory_Patient",
                        column: x => x.patient_id,
                        principalTable: "Patient",
                        principalColumn: "patient_id");
                    table.ForeignKey(
                        name: "FK_PatientMedicalHistory_Staff",
                        column: x => x.created_by,
                        principalTable: "Staff",
                        principalColumn: "staff_id");
                });

            // Visit table - pre-existing V3.0 table, not created in this migration

            // TestType table - pre-existing V3.0 table, not created in this migration

            migrationBuilder.CreateTable(
                name: "ContractPayment",
                columns: table => new
                {
                    contract_payment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    contract_invoice_id = table.Column<int>(type: "int", nullable: false),
                    payment_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    payment_method = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    reference_number = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractPayment", x => x.contract_payment_id);
                    table.ForeignKey(
                        name: "FK_ContractPayment_ContractInvoice",
                        column: x => x.contract_invoice_id,
                        principalTable: "ContractInvoice",
                        principalColumn: "contract_invoice_id");
                });

            // Payment table - pre-existing V3.0 table, not created in this migration

            // SampleTube table - pre-existing V3.0 table, not created in this migration

            migrationBuilder.CreateTable(
                name: "VisitCharge",
                columns: table => new
                {
                    charge_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    visit_id = table.Column<int>(type: "int", nullable: false),
                    charge_description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    charge_type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_by = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitCharge", x => x.charge_id);
                    table.ForeignKey(
                        name: "FK_VisitCharge_Staff",
                        column: x => x.created_by,
                        principalTable: "Staff",
                        principalColumn: "staff_id");
                    table.ForeignKey(
                        name: "FK_VisitCharge_Visit",
                        column: x => x.visit_id,
                        principalTable: "Visit",
                        principalColumn: "visit_id");
                });

            // TestComponent table - pre-existing V3.0 table, not created in this migration

            migrationBuilder.CreateTable(
                name: "TestProfileItem",
                columns: table => new
                {
                    profile_item_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    profile_id = table.Column<int>(type: "int", nullable: false),
                    testtype_id = table.Column<int>(type: "int", nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestProfileItem", x => x.profile_item_id);
                    table.ForeignKey(
                        name: "FK_TestProfileItem_TestProfile",
                        column: x => x.profile_id,
                        principalTable: "TestProfile",
                        principalColumn: "profile_id");
                    table.ForeignKey(
                        name: "FK_TestProfileItem_TestType",
                        column: x => x.testtype_id,
                        principalTable: "TestType",
                        principalColumn: "testtype_id");
                });

            // TestTypePrice table - pre-existing V3.0 table, not created in this migration

            // VisitTest table - pre-existing V3.0 table, not created in this migration

            // NormalRange table - pre-existing V3.0 table, not created in this migration

            // ReportCommentTemplate table - pre-existing V3.0 table, not created in this migration

            // CrossMatchTest table - pre-existing V3.0 table, not created in this migration

            migrationBuilder.CreateTable(
                name: "ExternalShipmentItem",
                columns: table => new
                {
                    shipment_item_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    shipment_id = table.Column<int>(type: "int", nullable: false),
                    visit_test_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalShipmentItem", x => x.shipment_item_id);
                    table.ForeignKey(
                        name: "FK_ExternalShipmentItem_ExternalShipment",
                        column: x => x.shipment_id,
                        principalTable: "ExternalShipment",
                        principalColumn: "shipment_id");
                    table.ForeignKey(
                        name: "FK_ExternalShipmentItem_VisitTest",
                        column: x => x.visit_test_id,
                        principalTable: "VisitTest",
                        principalColumn: "visit_test_id");
                });

            // MicrobiologyCulture table - pre-existing V3.0 table, not created in this migration

            // SemenAnalysis table - pre-existing V3.0 table, not created in this migration

            // TestResult table - pre-existing V3.0 table, not created in this migration

            // TestWorkflow table - pre-existing V3.0 table, not created in this migration

            // CrossMatchDonor table - pre-existing V3.0 table, not created in this migration

            // MicrobiologyOrganism table - pre-existing V3.0 table, not created in this migration

            // OrganismAntibiotic table - pre-existing V3.0 table, not created in this migration

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_shift_id",
                table: "Attendance",
                column: "shift_id");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_staff_id",
                table: "Attendance",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_StaffId1",
                table: "Attendance",
                column: "StaffId1");

            // Indexes for pre-existing V3.0 tables removed from this migration
            // IX_AuditLog_changed_by, IX_Company_scheme_id

            migrationBuilder.CreateIndex(
                name: "IX_ContractInvoice_company_id",
                table: "ContractInvoice",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_ContractInvoice_created_by",
                table: "ContractInvoice",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_ContractPayment_contract_invoice_id",
                table: "ContractPayment",
                column: "contract_invoice_id");

            // Indexes for pre-existing V3.0 tables removed from this migration
            // IX_CrossMatchDonor_crossmatch_id, IX_CrossMatchTest_tested_by, UQ__CrossMat__D6ECAC17BAD55465

            migrationBuilder.CreateIndex(
                name: "IX_ExternalShipment_created_by",
                table: "ExternalShipment",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalShipment_external_lab_id",
                table: "ExternalShipment",
                column: "external_lab_id");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalShipmentItem_shipment_id",
                table: "ExternalShipmentItem",
                column: "shipment_id");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalShipmentItem_visit_test_id",
                table: "ExternalShipmentItem",
                column: "visit_test_id");

            // Indexes for pre-existing V3.0 tables removed from this migration
            // IX_LabSettings_last_updated_by, IX_MicrobiologyCulture_inoculated_by,
            // IX_MicrobiologyCulture_read_by, UQ__Microbio__D6ECAC17AFF09907,
            // IX_MicrobiologyOrganism_culture_id, IX_NormalRange_component_id,
            // IX_OrganismAntibiotic_antibiotic_catalog_id, IX_OrganismAntibiotic_organism_id

            // Indexes for pre-existing V3.0 tables removed from this migration
            // IX_Patient_created_by, IX_Patient_Name, IX_Patient_Phone, UQ__Patient__58D46F1F4F7F8E9D

            migrationBuilder.CreateIndex(
                name: "IX_PatientMedicalHistory_created_by",
                table: "PatientMedicalHistory",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_PatientMedicalHistory_patient_id",
                table: "PatientMedicalHistory",
                column: "patient_id");

            // Indexes for pre-existing V3.0 tables removed from this migration
            // IX_Payment_received_by, IX_Payment_visit_id, UQ__Permissi__A98A808EC41BBC9C,
            // UQ__PriceSch__BB56A46B3A492F6F, IX_ReferralSource_scheme_id

            // Indexes for pre-existing V3.0 tables removed from this migration
            // IX_ReportCommentTemplate_category_id, IX_ReportCommentTemplate_component_id,
            // IX_ReportCommentTemplate_created_by, IX_ReportCommentTemplate_modified_by,
            // IX_ReportCommentTemplate_testtype_id, IX_SampleTube_collected_by,
            // IX_SampleTube_printed_by, IX_SampleTube_visit_id, UQ__SampleTu__6932170B90D6E91B

            // Indexes for pre-existing V3.0 tables removed from this migration
            // IX_SemenAnalysis_analyzed_by, UQ__SemenAna__D6ECAC172DED86C9,
            // UQ__Staff__F3DBC5720B13A882, IX_StaffPermission_granted_by,
            // IX_StaffPermission_permission_id, UQ_StaffPermission,
            // UQ__TestCate__BC9D1E7C823E4CF2, UQ_Component_Code,
            // IX_TestGroup_category_id, UQ__TestGrou__3180DCD1BE8C3F6E

            migrationBuilder.CreateIndex(
                name: "IX_TestProfile_created_by",
                table: "TestProfile",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_TestProfileItem_profile_id",
                table: "TestProfileItem",
                column: "profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_TestProfileItem_testtype_id",
                table: "TestProfileItem",
                column: "testtype_id");

            // Indexes for pre-existing V3.0 tables removed from this migration
            // IX_TestResult_component_id, IX_TestResult_entered_by,
            // IX_TestResult_last_modified_by, UQ_TestResult,
            // IX_TestType_group_id, UQ__TestType__2CB4DBF5D148A91E,
            // IX_TestTypePrice_testtype_id, UQ_SchemeTypePrice,
            // IX_TestWorkflow_performed_by, IX_TestWorkflow_visit_test_id

            // Indexes for pre-existing V3.0 tables removed from this migration
            // IX_Visit_Company, IX_Visit_Date, IX_Visit_Patient,
            // IX_Visit_receptionist_id, IX_Visit_Referral,
            // IX_Visit_scheme_id, IX_Visit_Status, UQ__Visit__6B282A41CE8E7529

            migrationBuilder.CreateIndex(
                name: "IX_VisitCharge_created_by",
                table: "VisitCharge",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_VisitCharge_visit_id",
                table: "VisitCharge",
                column: "visit_id");

            // Indexes for pre-existing V3.0 tables removed from this migration
            // IX_VisitTest_added_by, IX_VisitTest_external_lab_id,
                // IX_VisitTest_outsource_sent_by, IX_VisitTest_testtype_id,
                // IX_VisitTest_tube_id, UQ_VisitTest
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attendance");

            migrationBuilder.DropTable(
                name: "ContractPayment");

            migrationBuilder.DropTable(
                name: "ExternalShipmentItem");

            migrationBuilder.DropTable(
                name: "PatientMedicalHistory");

            migrationBuilder.DropTable(
                name: "TestProfileItem");

            migrationBuilder.DropTable(
                name: "VisitCharge");

            migrationBuilder.DropTable(
                name: "WorkShift");

            migrationBuilder.DropTable(
                name: "ContractInvoice");

            migrationBuilder.DropTable(
                name: "ExternalShipment");

            migrationBuilder.DropTable(
                name: "AntibioticCatalog");

            migrationBuilder.DropTable(
                name: "TestProfile");

            // The following tables are pre-existing V3.0 tables and should not be dropped in this migration
            // AuditLog, CrossMatchDonor, LabSettings, NormalRange, OrganismAntibiotic, Payment,
            // ReportCommentTemplate, SemenAnalysis, StaffPermission, TestResult, TestTypePrice,
            // TestWorkflow, CrossMatchTest, MicrobiologyOrganism, Permission, TestComponent,
            // MicrobiologyCulture, VisitTest, ExternalLab, TestType, SampleTube, TestGroup, Visit,
            // TestCategory, Company, Patient, ReferralSource, Staff, PriceScheme
        }
    }
}
