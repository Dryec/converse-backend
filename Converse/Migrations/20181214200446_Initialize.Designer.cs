﻿// <auto-generated />
using System;
using Converse.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Converse.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20181214200446_Initialize")]
    partial class Initialize
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Converse.Models.BlockedUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address");

                    b.Property<string>("BlockedAddress");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("blockedusers");
                });

            modelBuilder.Entity("Converse.Models.Chat", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreatedAt");

                    b.Property<bool>("IsGroup");

                    b.HasKey("Id");

                    b.ToTable("chats");
                });

            modelBuilder.Entity("Converse.Models.ChatMessage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address");

                    b.Property<DateTime>("BlockCreatedAt");

                    b.Property<long>("BlockId");

                    b.Property<int>("ChatId");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<int>("InternalId");

                    b.Property<string>("Message");

                    b.Property<DateTime>("TransactionCreatedAt");

                    b.Property<string>("TransactionHash");

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("ChatId");

                    b.HasIndex("UserId");

                    b.ToTable("chatmessages");
                });

            modelBuilder.Entity("Converse.Models.ChatSetting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address");

                    b.Property<int>("ChatId");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<string>("Description");

                    b.Property<bool>("IsPublic");

                    b.Property<string>("Name");

                    b.Property<string>("PictureUrl");

                    b.HasKey("Id");

                    b.HasIndex("ChatId")
                        .IsUnique();

                    b.ToTable("chatsettings");
                });

            modelBuilder.Entity("Converse.Models.ChatUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address");

                    b.Property<int>("ChatId");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<bool>("IsAdmin");

                    b.Property<DateTime>("JoinedAt");

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("ChatId");

                    b.HasIndex("UserId");

                    b.ToTable("chatusers");
                });

            modelBuilder.Entity("Converse.Models.Setting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Key");

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.ToTable("settings");
                });

            modelBuilder.Entity("Converse.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<string>("Nickname");

                    b.Property<string>("ProfilePictureUrl");

                    b.Property<string>("Status");

                    b.Property<DateTime>("StatusUpdatedAt");

                    b.HasKey("Id");

                    b.ToTable("users");
                });

            modelBuilder.Entity("Converse.Models.UserDeviceId", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreatedAt");

                    b.Property<string>("DeviceId");

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("userdeviceids");
                });

            modelBuilder.Entity("Converse.Models.UserReceivedToken", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<string>("Ip");

                    b.Property<int>("ReceivedTokens");

                    b.HasKey("Id");

                    b.ToTable("userreceivedtokens");
                });

            modelBuilder.Entity("Converse.Models.BlockedUser", b =>
                {
                    b.HasOne("Converse.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Converse.Models.ChatMessage", b =>
                {
                    b.HasOne("Converse.Models.Chat", "Chat")
                        .WithMany("Messages")
                        .HasForeignKey("ChatId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Converse.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Converse.Models.ChatSetting", b =>
                {
                    b.HasOne("Converse.Models.Chat", "Chat")
                        .WithOne("Setting")
                        .HasForeignKey("Converse.Models.ChatSetting", "ChatId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Converse.Models.ChatUser", b =>
                {
                    b.HasOne("Converse.Models.Chat", "Chat")
                        .WithMany("Users")
                        .HasForeignKey("ChatId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Converse.Models.User", "User")
                        .WithMany("ChatUsers")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Converse.Models.UserDeviceId", b =>
                {
                    b.HasOne("Converse.Models.User", "User")
                        .WithMany("UserDeviceIds")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
