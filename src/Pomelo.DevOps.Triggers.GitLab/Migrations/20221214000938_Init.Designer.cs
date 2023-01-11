﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pomelo.DevOps.Triggers.GitLab.Models;

#nullable disable

namespace Pomelo.DevOps.Triggers.GitLab.Migrations
{
    [DbContext(typeof(TriggerContext))]
    [Migration("20221214000938_Init")]
    partial class Init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.11");

            modelBuilder.Entity("Pomelo.DevOps.Triggers.GitLab.Models.Trigger", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("ArgumentsJson")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Enabled")
                        .HasColumnType("INTEGER");

                    b.Property<string>("GitLabNamespace")
                        .HasMaxLength(64)
                        .HasColumnType("TEXT");

                    b.Property<string>("GitLabProject")
                        .HasMaxLength(64)
                        .HasColumnType("TEXT");

                    b.Property<string>("JobDescriptionTemplate")
                        .HasColumnType("TEXT");

                    b.Property<string>("JobNameTemplate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("PomeloDevOpsPipeline")
                        .HasMaxLength(64)
                        .HasColumnType("TEXT");

                    b.Property<string>("PomeloDevOpsProject")
                        .HasMaxLength(64)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Enabled", "GitLabNamespace", "GitLabProject");

                    b.ToTable("Triggers");
                });
#pragma warning restore 612, 618
        }
    }
}