#!/usr/bin/env python3
"""Oyunun ses efektlerini sentezler (kayıt ya da dış kaynak kullanmaz; yalnızca standart kütüphane).

Kullanım: python3 Tools/Audio/make_sfx.py [çıktı klasörü]
Varsayılan çıktı: Assets/_Game/Audio/Sfx. Çıktı 44,1 kHz, 16 bit, tek kanal WAV'dır.
Sesi değiştirmek için buradaki sayıları değiştir, betiği çalıştır, sonra Unity'de
"Out of Office > Placeholder İçeriği Yeniden Oluştur" ile SfxLibrary'yi yenile.
"""
import math
import os
import random
import struct
import sys
import wave

RATE = 44100
TAU = 2.0 * math.pi


def silence(seconds):
    return [0.0] * int(RATE * seconds)


def tone(seconds, freq_start, freq_end=None, shape="sine", curve=1.0, vibrato=(0.0, 0.0)):
    """Frekansı baştan sona kayan tek ses. curve > 1: kayma sona doğru hızlanır."""
    freq_end = freq_start if freq_end is None else freq_end
    count = int(RATE * seconds)
    out = []
    phase = 0.0
    for i in range(count):
        t = i / max(count - 1, 1)
        freq = freq_start + (freq_end - freq_start) * (t ** curve)
        freq *= 1.0 + vibrato[1] * math.sin(TAU * vibrato[0] * i / RATE)
        phase += freq / RATE
        x = phase % 1.0
        if shape == "sine":
            value = math.sin(TAU * x)
        elif shape == "triangle":
            value = 4.0 * abs(x - 0.5) - 1.0
        elif shape == "square":
            value = 0.6 if x < 0.5 else -0.6
        else:  # yumuşak testere
            value = (2.0 * x - 1.0) * 0.7
        out.append(value)
    return out


def noise(seconds, cutoff_start, cutoff_end, rng):
    """Tek kutuplu alçak geçiren süzgeçten geçmiş gürültü; kesim frekansı baştan sona kayar."""
    count = int(RATE * seconds)
    out = []
    state = 0.0
    for i in range(count):
        t = i / max(count - 1, 1)
        cutoff = cutoff_start + (cutoff_end - cutoff_start) * t
        alpha = 1.0 - math.exp(-TAU * cutoff / RATE)
        state += alpha * (rng.uniform(-1.0, 1.0) - state)
        out.append(state * 2.5)
    return out


def envelope(samples, attack=0.005, decay=6.0, hold=0.0):
    """Hızlı giriş, sonra üstel sönüm. decay büyüdükçe ses daha çabuk söner."""
    count = len(samples)
    attack_n = max(1, int(RATE * attack))
    hold_n = int(RATE * hold)
    out = []
    for i, value in enumerate(samples):
        gain = min(1.0, i / attack_n)
        if i > attack_n + hold_n:
            gain *= math.exp(-decay * (i - attack_n - hold_n) / RATE)
        # Son 4 ms'de sıfıra iner: klip sonunda çıt sesi olmaz.
        gain *= min(1.0, (count - i) / (RATE * 0.004))
        out.append(value * gain)
    return out


def mix(*layers):
    out = [0.0] * max(len(layer) for layer in layers)
    for layer in layers:
        for i, value in enumerate(layer):
            out[i] += value
    return out


def gain(samples, amount):
    return [value * amount for value in samples]


def sequence(*parts):
    out = []
    for part in parts:
        out.extend(part)
    return out


def bell(seconds, freq, decay):
    """Zil tınısı: temel ses + iki sönük üst ton."""
    return mix(
        envelope(tone(seconds, freq), 0.002, decay),
        gain(envelope(tone(seconds, freq * 2.0), 0.002, decay * 1.6), 0.35),
        gain(envelope(tone(seconds, freq * 3.01), 0.002, decay * 2.4), 0.15),
    )


def build(rng):
    return {
        # Zıplama: yukarı kayan yumuşak "vup".
        "sfx_jump": mix(
            envelope(tone(0.2, 280, 760, "sine", 0.7), 0.008, 9.0),
            gain(envelope(tone(0.2, 560, 1520, "triangle", 0.7), 0.008, 14.0), 0.2),
        ),
        # Kayma: aşağı süzülen hışırtı.
        "sfx_slide": envelope(noise(0.32, 5200, 500, rng), 0.03, 7.0, 0.05),
        # Şerit değiştirme: kısa, hafif bir "fış".
        "sfx_lane": mix(
            gain(envelope(noise(0.11, 3200, 1400, rng), 0.01, 26.0), 0.8),
            gain(envelope(tone(0.11, 520, 390), 0.004, 30.0), 0.35),
        ),
        # Altın: iki notalı çın.
        "sfx_coin": sequence(bell(0.055, 1319, 8.0), bell(0.3, 1976, 11.0)),
        # Güçlendirme: yükselen dört nota.
        "sfx_powerup": sequence(
            envelope(tone(0.07, 523, shape="triangle"), 0.004, 6.0),
            envelope(tone(0.07, 659, shape="triangle"), 0.004, 6.0),
            envelope(tone(0.07, 784, shape="triangle"), 0.004, 6.0),
            mix(envelope(tone(0.4, 1047, shape="triangle"), 0.004, 6.5), gain(bell(0.4, 2093, 9.0), 0.3)),
        ),
        # Tökezleme: boğuk bir "güm" ve kısa çarpma gürültüsü.
        "sfx_stumble": mix(
            envelope(tone(0.26, 170, 48, "sine", 0.5), 0.003, 12.0),
            gain(envelope(noise(0.09, 2600, 700, rng), 0.002, 30.0), 0.7),
        ),
        # Kalkan kırılması: cam şıngırtısı.
        "sfx_shield_break": mix(
            gain(envelope(noise(0.3, 9000, 3500, rng), 0.002, 14.0), 0.5),
            sequence(bell(0.07, 2637, 14.0), bell(0.08, 2093, 14.0), bell(0.22, 1568, 12.0)),
        ),
        # Yakalanma: alçalan üç nota, hafif titrek (çizgi film "kaybettin" sesi).
        "sfx_caught": sequence(
            envelope(tone(0.2, 392, 370, "saw", vibrato=(7.0, 0.012)), 0.01, 3.0),
            envelope(tone(0.2, 330, 311, "saw", vibrato=(7.0, 0.012)), 0.01, 3.0),
            envelope(tone(0.5, 262, 196, "saw", 1.6, vibrato=(7.0, 0.02)), 0.01, 4.5),
        ),
        # Arayüz düğmesi: kısa "tık".
        "sfx_click": mix(
            envelope(tone(0.06, 880, 660), 0.002, 45.0),
            gain(envelope(tone(0.06, 1760), 0.002, 70.0), 0.3),
        ),
    }


def write_wav(path, samples, peak=0.8):
    loudest = max(abs(value) for value in samples) or 1.0
    scale = peak / loudest
    with wave.open(path, "wb") as out:
        out.setnchannels(1)
        out.setsampwidth(2)
        out.setframerate(RATE)
        out.writeframes(b"".join(struct.pack("<h", int(max(-1.0, min(1.0, value * scale)) * 32767)) for value in samples))


def main():
    root = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
    target = sys.argv[1] if len(sys.argv) > 1 else os.path.join(root, "Assets", "_Game", "Audio", "Sfx")
    os.makedirs(target, exist_ok=True)
    # Sabit tohum: betik her çalıştığında aynı dosyaları üretir.
    for name, samples in build(random.Random(9000)).items():
        write_wav(os.path.join(target, name + ".wav"), samples)
        print(f"{name}.wav  {len(samples) / RATE:.2f} sn")


if __name__ == "__main__":
    main()
