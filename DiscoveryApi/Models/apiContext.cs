using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;


namespace DiscoveryApi.Models
{
    public partial class apiContext : DbContext
    {
        public virtual DbSet<ApiKeys> ApiKeys { get; set; }
        public virtual DbSet<ServerEvents> ServerEvents { get; set; }
        public virtual DbSet<ServerFactions> ServerFactions { get; set; }
        public virtual DbSet<ServerNames> ServerNames { get; set; }
        public virtual DbSet<ServerPlayercounts> ServerPlayercounts { get; set; }
        public virtual DbSet<ServerSessions> ServerSessions { get; set; }
        public virtual DbSet<ServerSessionsConndata> ServerSessionsConndata { get; set; }
        public virtual DbSet<ServerSessionsSystems> ServerSessionsSystems { get; set; }

        public static IConfigurationRoot Configuration { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");
            Configuration = builder.Build();

            optionsBuilder.UseMySql(Configuration.GetConnectionString("ApiConnection"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApiKeys>(entity =>
            {
                entity.ToTable("api_keys");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Key)
                    .IsRequired()
                    .HasColumnName("key")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Owner)
                    .HasColumnName("owner")
                    .HasColumnType("varchar(255)");
            });

            modelBuilder.Entity<ServerEvents>(entity =>
            {
                entity.ToTable("server_events");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CurrentCount)
                    .HasColumnName("current_count")
                    .HasColumnType("int(11)");

                entity.Property(e => e.EventDescription)
                    .IsRequired()
                    .HasColumnName("event_description")
                    .HasColumnType("text");

                entity.Property(e => e.EventName)
                    .IsRequired()
                    .HasColumnName("event_name")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.ExpectedCount)
                    .HasColumnName("expected_count")
                    .HasColumnType("int(11)");

                entity.Property(e => e.IsActive)
                    .HasColumnName("is_active")
                    .HasColumnType("tinyint(1)");

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasColumnType("int(11)");
            });

            modelBuilder.Entity<ServerFactions>(entity =>
            {
                entity.HasKey(e => e.FactionTag)
                    .HasName("PK_server_factions");

                entity.ToTable("server_factions");

                entity.Property(e => e.FactionTag)
                    .HasColumnName("faction_tag")
                    .HasColumnType("varchar(24)");

                entity.Property(e => e.FactionAdded)
                    .HasColumnName("faction_added")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.FactionName)
                    .IsRequired()
                    .HasColumnName("faction_name")
                    .HasColumnType("text");

                entity.Property(e => e.ItemEquipId)
                    .IsRequired()
                    .HasColumnName("item_equip_id")
                    .HasColumnType("varchar(128)");

                entity.Property(e => e.Warned)
                    .HasColumnName("warned")
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("0");
            });

            modelBuilder.Entity<ServerNames>(entity =>
            {
                entity.HasKey(e => e.Nickname)
                    .HasName("PK_server_names");

                entity.ToTable("server_names");

                entity.HasIndex(e => e.Category)
                    .HasName("category");

                entity.Property(e => e.Nickname)
                    .HasColumnName("nickname")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Category)
                    .IsRequired()
                    .HasColumnName("category")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.NameEn)
                    .IsRequired()
                    .HasColumnName("name_en")
                    .HasColumnType("text");
            });

            modelBuilder.Entity<ServerPlayercounts>(entity =>
            {
                entity.HasKey(e => e.Date)
                    .HasName("PK_server_playercounts");

                entity.ToTable("server_playercounts");

                entity.Property(e => e.Date).HasColumnName("date");

                entity.Property(e => e.PlayerCount)
                    .HasColumnName("player_count")
                    .HasColumnType("smallint(6)");
            });

            modelBuilder.Entity<ServerSessions>(entity =>
            {
                entity.HasKey(e => e.SessionId)
                    .HasName("PK_server_sessions");

                entity.ToTable("server_sessions");

                entity.HasIndex(e => e.PlayerName)
                    .HasName("player_name");

                entity.HasIndex(e => e.SessionEnd)
                    .HasName("session_end");

                entity.HasIndex(e => e.SessionIp)
                    .HasName("session_ip");

                entity.HasIndex(e => e.SessionStart)
                    .HasName("session_start");

                entity.Property(e => e.SessionId)
                    .HasColumnName("session_id")
                    .HasColumnType("int(11) unsigned");

                entity.Property(e => e.PlayerId)
                    .IsRequired()
                    .HasColumnName("player_id")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.PlayerLagAvg)
                    .HasColumnName("player_lag_avg")
                    .HasColumnType("int(6) unsigned");

                entity.Property(e => e.PlayerLastShip)
                    .IsRequired()
                    .HasColumnName("player_last_ship")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.PlayerLossAvg)
                    .HasColumnName("player_loss_avg")
                    .HasColumnType("int(6) unsigned");

                entity.Property(e => e.PlayerName)
                    .IsRequired()
                    .HasColumnName("player_name")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.PlayerPingAvg)
                    .HasColumnName("player_ping_avg")
                    .HasColumnType("int(6) unsigned");

                entity.Property(e => e.SessionEnd).HasColumnName("session_end");

                entity.Property(e => e.SessionIp)
                    .IsRequired()
                    .HasColumnName("session_ip")
                    .HasColumnType("varchar(24)");

                entity.Property(e => e.SessionStart).HasColumnName("session_start");
            });

            modelBuilder.Entity<ServerSessionsConndata>(entity =>
            {
                entity.HasKey(e => new { e.SessionId, e.SessionRec })
                    .HasName("PK_server_sessions_conndata");

                entity.ToTable("server_sessions_conndata");

                entity.HasIndex(e => e.PlayerShip)
                    .HasName("player_ship");

                entity.HasIndex(e => e.PlayerSystem)
                    .HasName("player_system");

                entity.Property(e => e.SessionId)
                    .HasColumnName("session_id")
                    .HasColumnType("int(11) unsigned")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.SessionRec)
                    .HasColumnName("session_rec")
                    .HasDefaultValueSql("0000-00-00 00:00:00");

                entity.Property(e => e.PlayerLag)
                    .HasColumnName("player_lag")
                    .HasColumnType("int(6) unsigned");

                entity.Property(e => e.PlayerLoss)
                    .HasColumnName("player_loss")
                    .HasColumnType("int(6) unsigned");

                entity.Property(e => e.PlayerPing)
                    .HasColumnName("player_ping")
                    .HasColumnType("int(6) unsigned");

                entity.Property(e => e.PlayerShip)
                    .IsRequired()
                    .HasColumnName("player_ship")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.PlayerSystem)
                    .IsRequired()
                    .HasColumnName("player_system")
                    .HasColumnType("varchar(255)");
            });

            modelBuilder.Entity<ServerSessionsSystems>(entity =>
            {
                entity.HasKey(e => new { e.SessionId, e.SystemId, e.VisitDate })
                    .HasName("PK_server_sessions_systems");

                entity.ToTable("server_sessions_systems");

                entity.Property(e => e.SessionId)
                    .HasColumnName("session_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.SystemId)
                    .HasColumnName("system_id")
                    .HasColumnType("varchar(36)");

                entity.Property(e => e.VisitDate).HasColumnName("visit_date");

                entity.Property(e => e.VisitDuration)
                    .HasColumnName("visit_duration")
                    .HasColumnType("int(11)");
            });
        }
    }
}