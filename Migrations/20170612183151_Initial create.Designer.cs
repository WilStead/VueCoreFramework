﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using MVCCoreVue.Data;
using MVCCoreVue.Models;

namespace MVCCoreVue.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20170612183151_Initial create")]
    partial class Initialcreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("MVCCoreVue.Models.Airline", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AllPermissions");

                    b.Property<Guid?>("CountryId");

                    b.Property<DateTime>("CreationTimestamp");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<DateTime>("UpdateTimestamp");

                    b.HasKey("Id");

                    b.HasIndex("CountryId");

                    b.ToTable("Airline");
                });

            modelBuilder.Entity("MVCCoreVue.Models.AirlineCountry", b =>
                {
                    b.Property<Guid>("CountryId");

                    b.Property<Guid>("AirlineId");

                    b.HasKey("CountryId", "AirlineId");

                    b.HasIndex("AirlineId");

                    b.ToTable("AirlineCountry");
                });

            modelBuilder.Entity("MVCCoreVue.Models.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<bool>("AdminLocked");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<DateTime>("LastEmailChange");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NewEmail");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("OldEmail");

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("MVCCoreVue.Models.City", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AllPermissions");

                    b.Property<Guid>("CitiesCountryId");

                    b.Property<DateTime>("CreationTimestamp");

                    b.Property<DateTime>("LocalTimeAtGMTMidnight");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<int>("Population");

                    b.Property<int>("Transit");

                    b.Property<DateTime>("UpdateTimestamp");

                    b.HasKey("Id");

                    b.HasIndex("CitiesCountryId");

                    b.ToTable("Cities");
                });

            modelBuilder.Entity("MVCCoreVue.Models.Country", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AllPermissions");

                    b.Property<Guid?>("CapitolId");

                    b.Property<DateTime>("CreationTimestamp");

                    b.Property<double>("EpiIndex");

                    b.Property<string>("FlagPrimaryColor");

                    b.Property<Guid>("LeaderId");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<DateTime>("UpdateTimestamp");

                    b.HasKey("Id");

                    b.HasIndex("CapitolId");

                    b.ToTable("Countries");
                });

            modelBuilder.Entity("MVCCoreVue.Models.Leader", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Age");

                    b.Property<string>("AllPermissions");

                    b.Property<DateTime>("Birthdate");

                    b.Property<DateTime>("CreationTimestamp");

                    b.Property<Guid>("LeaderCountryId");

                    b.Property<int>("MaritalStatus");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<long>("TimeInOfficeTicks");

                    b.Property<DateTime>("UpdateTimestamp");

                    b.HasKey("Id");

                    b.HasIndex("LeaderCountryId")
                        .IsUnique();

                    b.ToTable("Leaders");
                });

            modelBuilder.Entity("MVCCoreVue.Models.Log", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Callsite");

                    b.Property<string>("Exception");

                    b.Property<string>("Level")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<string>("Logger")
                        .HasMaxLength(250);

                    b.Property<string>("Message");

                    b.Property<DateTime>("Timestamp");

                    b.Property<string>("Url");

                    b.HasKey("Id");

                    b.ToTable("Logs");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Claims")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("MVCCoreVue.Models.ApplicationUser")
                        .WithMany("Claims")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("MVCCoreVue.Models.ApplicationUser")
                        .WithMany("Logins")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Users")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("MVCCoreVue.Models.ApplicationUser")
                        .WithMany("Roles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MVCCoreVue.Models.Airline", b =>
                {
                    b.HasOne("MVCCoreVue.Models.Country")
                        .WithMany("Airlines")
                        .HasForeignKey("CountryId");
                });

            modelBuilder.Entity("MVCCoreVue.Models.AirlineCountry", b =>
                {
                    b.HasOne("MVCCoreVue.Models.Airline", "Airline")
                        .WithMany("AirlineCountries")
                        .HasForeignKey("AirlineId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("MVCCoreVue.Models.Country", "Country")
                        .WithMany("CountryAirlines")
                        .HasForeignKey("CountryId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MVCCoreVue.Models.City", b =>
                {
                    b.HasOne("MVCCoreVue.Models.Country", "CitiesCountry")
                        .WithMany("Cities")
                        .HasForeignKey("CitiesCountryId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MVCCoreVue.Models.Country", b =>
                {
                    b.HasOne("MVCCoreVue.Models.City", "Capitol")
                        .WithMany()
                        .HasForeignKey("CapitolId");
                });

            modelBuilder.Entity("MVCCoreVue.Models.Leader", b =>
                {
                    b.HasOne("MVCCoreVue.Models.Country", "Country")
                        .WithOne("Leader")
                        .HasForeignKey("MVCCoreVue.Models.Leader", "LeaderCountryId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}