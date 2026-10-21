"""Đường dẫn và tham số dùng chung."""
from dataclasses import dataclass
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DATA = ROOT / "data"
RAW = DATA / "raw"
EXAMPLE = DATA / "example"
OUTPUT = ROOT / "outputs"
BASELINE_DIR = OUTPUT / "baseline"
PHOBERT_DIR = OUTPUT / "phobert"

@dataclass(frozen=True)
class TrainConfig:
    seed: int = 42
    max_length: int = 192
    batch_size: int = 8
    epochs: int = 2
    learning_rate: float = 2e-5
    model_name: str = "vinai/phobert-base"

LABEL_NAMES = {0: "Không độc hại", 1: "Độc hại"}
