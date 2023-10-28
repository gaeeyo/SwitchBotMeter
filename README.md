# SwitchBotMeter について

SwitchBot の温度湿度計から温度と湿度を取得して表示するコンソールアプリケーション。

Linux のツールはあったけど、Windows 用が見つけられなかったので作成。


本当は Win32 で作りたかったけど、難しそうだったので諦めて .NET Core 3.1 を使用。

とはいえ、C# も .NET Core 3.1 もほとんど経験がないので、とんでもない実装をしている可能性があります。

# 動作環境

- Windows 10 Build 19041
- .NET 6.0 (Windows)

# ビルド

```
dotnet publish --configuration Release
```

# 使い方

起動すると、空白切りでデバイスのアドレス、温度、湿度、バッテリー残量(%)、デバイスの種類が表示されます。

デフォルトでは1回取得したら終了します。

デフォルトのタイムアウトは120秒です。

1回だけ取得。
```cmd
C:\> SwitchBotMeter
FEDBB31721C2 27.8 56 100 Meter
F3AE13927590 22.8 77 100 OutdoorMeter
FEF7187D856A 26.4 44 100 MeterPlus
```

タイムアウトを10秒に設定して取得。
```cmd
C:\> SwitchBotMeter --timeout 10
```

1分間取得し続ける。
```cmd
C:\> SwitchBotMeter --timeout 60 --limit 0
```

無限に取得を繰り返す。
```cmd
C:\> SwitchBotMeter --timeout 0 --limit 0
```

# 参考

以下の情報を参考にしました。ありがとうございます。

- https://github.com/OpenWonderLabs/python-host/wiki/Meter-BLE-open-API
- https://qiita.com/warpzone/items/11ec9bef21f5b965bce3
