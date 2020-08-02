# SwitchBotMeter について

SwitchBot の温度湿度計から温度と湿度を取得して表示するコンソールアプリケーション。
Linux のツールはあったけど、Windows 用が見つけられなかったので作成。

本当は Win32 で作りたかったけど、難しそうだったので諦めて .NET Core 3.1 を使用。
とはいえ、C# も .NET Core 3.1 もほとんど経験がないので、とんでもない実装をしている可能性があります。

# 使い方

起動すると、空白切りでデバイスのアドレス、温度、湿度が表示されます。
デフォルトでは1回取得したら終了します。
デフォルトのタイムアウトは120秒です。
```cmd
C:\> SwitchBotMeter
FEDBB31721C2 26.1 58
```