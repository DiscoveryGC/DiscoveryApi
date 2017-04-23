using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace DiscoveryApi.DAL
{
    public partial class apiContext : DbContext
    {
        public virtual DbSet<ApiKeys> ApiKeys { get; set; }
        public virtual DbSet<GameRegions> GameRegions { get; set; }
        public virtual DbSet<GameSystems> GameSystems { get; set; }
        public virtual DbSet<ServerEvents> ServerEvents { get; set; }
        public virtual DbSet<ServerFactions> ServerFactions { get; set; }
        public virtual DbSet<ServerNames> ServerNames { get; set; }
        public virtual DbSet<ServerPlayercounts> ServerPlayercounts { get; set; }
        public virtual DbSet<ServerSessions> ServerSessions { get; set; }
        public virtual DbSet<ServerSessionsDataConn> ServerSessionsDataConn { get; set; }

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

                entity.Property(e => e.Admin)
                    .HasColumnName("admin")
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.Key)
                    .IsRequired()
                    .HasColumnName("key")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Owner)
                    .HasColumnName("owner")
                    .HasColumnType("varchar(255)");
            });

            modelBuilder.Entity<GameRegions>(entity =>
            {
                entity.ToTable("game_regions");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Fullname)
                    .IsRequired()
                    .HasColumnName("fullname")
                    .HasColumnType("varchar(255)");
            });

            modelBuilder.Entity<GameSystems>(entity =>
            {
                entity.ToTable("game_systems");

                entity.HasIndex(e => e.RegionId)
                    .HasName("region_id");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Fullname)
                    .IsRequired()
                    .HasColumnName("fullname")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Nickname)
                    .IsRequired()
                    .HasColumnName("nickname")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.RegionId)
                    .HasColumnName("region_id")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.Region)
                    .WithMany(p => p.GameSystems)
                    .HasForeignKey(d => d.RegionId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("game_systems_ibfk_1");
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

                //entity.HasMany(d => d.ServerSessionsDataConn)
                //    .WithOne(p => p.Session)
                //    .HasForeignKey(d => d.SessionId)
                //    .OnDelete(DeleteBehavior.Restrict)
                //    .HasConstraintName("server_sessions_data_conn_ibfk_1");
           
            });

            modelBuilder.Entity<ServerSessionsDataConn>(entity =>
            {
                entity.ToTable("server_sessions_data_conn");

                entity.HasIndex(e => e.SessionId)
                    .HasName("session_id");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Lag)
                    .HasColumnName("lag")
                    .HasColumnType("int(11) unsigned");

                entity.Property(e => e.Stamp).HasColumnName("stamp");

                entity.Property(e => e.Loss)
                    .HasColumnName("loss")
                    .HasColumnType("int(11) unsigned");

                entity.Property(e => e.Ping)
                    .HasColumnName("ping")
                    .HasColumnType("int(11) unsigned");

                entity.Property(e => e.SessionId)
                    .HasColumnName("session_id")
                    .HasColumnType("int(11) unsigned");

                entity.Property(e => e.Ship)
                    .IsRequired()
                    .HasColumnName("ship")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Location)
                    .IsRequired()
                    .HasColumnName("location")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Duration)
                    .HasColumnName("duration")
                    .HasColumnType("int(11) unsigned");

                entity.HasOne(d => d.Session)
                    .WithMany(p => p.ServerSessionsDataConn)
                    .HasForeignKey(d => d.SessionId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("server_sessions_data_conn_ibfk_1");
            });
        }
    }
}