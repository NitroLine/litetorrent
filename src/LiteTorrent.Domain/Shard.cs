﻿using LiteTorrent.Infra;

namespace LiteTorrent.Domain;

public record Shard(Hash Hash, ReadOnlyMemory<byte> Data);