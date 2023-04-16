﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TwitchBot.Data;

#nullable disable

namespace TwitchBot.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.4");

            modelBuilder.Entity("TwitchBot.Data.Battle", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Amount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FirstPlayer")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Battles");
                });

            modelBuilder.Entity("TwitchBot.Data.BotConfig", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("BotConfigs");
                });

            modelBuilder.Entity("TwitchBot.Data.Dailyspin", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Lastspin")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Dailyspins");
                });

            modelBuilder.Entity("TwitchBot.Data.FirstToWin", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Amount")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("FirstToWins");
                });

            modelBuilder.Entity("TwitchBot.Data.LastGame", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Game")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("LastGames");
                });

            modelBuilder.Entity("TwitchBot.Data.LoyaltyPoint", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Amount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Chatter")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("LoyaltyPoints");
                });

            modelBuilder.Entity("TwitchBot.Data.PlayToWin", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("RewardAmount")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("PlayToWins");
                });

            modelBuilder.Entity("TwitchBot.Data.PlayToWinPlayer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Chatter")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("PlayToWinId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("PlayToWinId");

                    b.ToTable("PlayToWinPlayers");
                });

            modelBuilder.Entity("TwitchBot.Data.Raffle", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("RewardAmount")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Raffles");
                });

            modelBuilder.Entity("TwitchBot.Data.RaffleTicket", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Chatter")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("RaffleId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("RaffleId");

                    b.ToTable("RaffleTickets");
                });

            modelBuilder.Entity("TwitchBot.Data.RandomDrop", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Amount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Chatter")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("RandomDrops");
                });

            modelBuilder.Entity("TwitchBot.Data.RollDice", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Amount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Chatter")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Opponent")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("RollDices");
                });

            modelBuilder.Entity("TwitchBot.Data.TwitchCode", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AccessToken")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("RefreshToken")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("TwitchCodes");
                });

            modelBuilder.Entity("TwitchBot.Data.PlayToWinPlayer", b =>
                {
                    b.HasOne("TwitchBot.Data.PlayToWin", "PlayToWin")
                        .WithMany("Players")
                        .HasForeignKey("PlayToWinId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("PlayToWin");
                });

            modelBuilder.Entity("TwitchBot.Data.RaffleTicket", b =>
                {
                    b.HasOne("TwitchBot.Data.Raffle", "Raffle")
                        .WithMany("Tickets")
                        .HasForeignKey("RaffleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Raffle");
                });

            modelBuilder.Entity("TwitchBot.Data.PlayToWin", b =>
                {
                    b.Navigation("Players");
                });

            modelBuilder.Entity("TwitchBot.Data.Raffle", b =>
                {
                    b.Navigation("Tickets");
                });
#pragma warning restore 612, 618
        }
    }
}
