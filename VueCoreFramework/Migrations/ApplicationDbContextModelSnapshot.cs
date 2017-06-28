using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using VueCoreFramework.Data;
using VueCoreFramework.Models;

namespace VueCoreFramework.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
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

            modelBuilder.Entity("VueCoreFramework.Models.Airline", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool?>("International");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Airlines");
                });

            modelBuilder.Entity("VueCoreFramework.Models.AirlineCountry", b =>
                {
                    b.Property<Guid>("CountryId");

                    b.Property<Guid>("AirlineId");

                    b.HasKey("CountryId", "AirlineId");

                    b.HasIndex("AirlineId");

                    b.ToTable("AirlineCountry");
                });

            modelBuilder.Entity("VueCoreFramework.Models.ApplicationUser", b =>
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

            modelBuilder.Entity("VueCoreFramework.Models.City", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid?>("CountryCapitolId");

                    b.Property<Guid>("CountryId");

                    b.Property<DateTime>("LocalTimeAtGMTMidnight");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<int>("Population");

                    b.Property<int>("Transit");

                    b.HasKey("Id");

                    b.HasIndex("CountryCapitolId")
                        .IsUnique();

                    b.HasIndex("CountryId");

                    b.ToTable("Cities");
                });

            modelBuilder.Entity("VueCoreFramework.Models.Country", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<double?>("EpiIndex");

                    b.Property<string>("FlagPrimaryColor");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Countries");
                });

            modelBuilder.Entity("VueCoreFramework.Models.Leader", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Age");

                    b.Property<DateTime?>("Birthdate");

                    b.Property<Guid>("CountryId");

                    b.Property<int>("MaritalStatus");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<long?>("TimeInOfficeTicks");

                    b.HasKey("Id");

                    b.HasIndex("CountryId")
                        .IsUnique();

                    b.ToTable("Leaders");
                });

            modelBuilder.Entity("VueCoreFramework.Models.Log", b =>
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

            modelBuilder.Entity("VueCoreFramework.Models.Message", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Content")
                        .HasMaxLength(125);

                    b.Property<string>("GroupRecipientId");

                    b.Property<string>("GroupRecipientName");

                    b.Property<bool>("IsSystemMessage");

                    b.Property<bool>("Received");

                    b.Property<bool>("RecipientDeleted");

                    b.Property<bool>("SenderDeleted");

                    b.Property<string>("SenderId");

                    b.Property<string>("SenderUsername");

                    b.Property<string>("SingleRecipientId");

                    b.Property<string>("SingleRecipientName");

                    b.Property<DateTime>("Timestamp")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.HasKey("Id");

                    b.HasIndex("GroupRecipientId");

                    b.HasIndex("SenderId");

                    b.HasIndex("SingleRecipientId");

                    b.ToTable("Messages");
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
                    b.HasOne("VueCoreFramework.Models.ApplicationUser")
                        .WithMany("Claims")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("VueCoreFramework.Models.ApplicationUser")
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

                    b.HasOne("VueCoreFramework.Models.ApplicationUser")
                        .WithMany("Roles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("VueCoreFramework.Models.AirlineCountry", b =>
                {
                    b.HasOne("VueCoreFramework.Models.Airline", "Airline")
                        .WithMany("Countries")
                        .HasForeignKey("AirlineId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("VueCoreFramework.Models.Country", "Country")
                        .WithMany("Airlines")
                        .HasForeignKey("CountryId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("VueCoreFramework.Models.City", b =>
                {
                    b.HasOne("VueCoreFramework.Models.Country", "CountryCapitol")
                        .WithOne("Capitol")
                        .HasForeignKey("VueCoreFramework.Models.City", "CountryCapitolId");

                    b.HasOne("VueCoreFramework.Models.Country", "Country")
                        .WithMany("Cities")
                        .HasForeignKey("CountryId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("VueCoreFramework.Models.Leader", b =>
                {
                    b.HasOne("VueCoreFramework.Models.Country", "Country")
                        .WithOne("Leader")
                        .HasForeignKey("VueCoreFramework.Models.Leader", "CountryId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("VueCoreFramework.Models.Message", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole", "GroupRecipient")
                        .WithMany()
                        .HasForeignKey("GroupRecipientId");

                    b.HasOne("VueCoreFramework.Models.ApplicationUser", "Sender")
                        .WithMany()
                        .HasForeignKey("SenderId");

                    b.HasOne("VueCoreFramework.Models.ApplicationUser", "SingleRecipient")
                        .WithMany()
                        .HasForeignKey("SingleRecipientId");
                });
        }
    }
}
