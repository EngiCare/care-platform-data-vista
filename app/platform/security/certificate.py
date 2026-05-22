# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Certificate utilities — mirrors CertificateUtil.cs.

On non-Windows or when no cert store is available, generates a self-signed
RSA key pair for JWT signing.  This matches the C# Linux fallback behavior.
"""

from __future__ import annotations

import logging
from functools import lru_cache

from cryptography import x509
from cryptography.hazmat.primitives import hashes, serialization
from cryptography.hazmat.primitives.asymmetric import rsa
from cryptography.x509.oid import NameOID
import datetime

logger = logging.getLogger(__name__)


@lru_cache(maxsize=1)
def get_signing_key() -> rsa.RSAPrivateKey:
    """Get (or create) a self-signed RSA private key for JWT signing.

    Cached so every call returns the same key pair — prevents IDX10503-style
    sign/verify mismatches.
    """
    logger.info("Generating self-signed RSA key for JWT signing")
    key = rsa.generate_private_key(public_exponent=65537, key_size=2048)
    return key


def get_private_key_pem() -> bytes:
    key = get_signing_key()
    return key.private_bytes(
        encoding=serialization.Encoding.PEM,
        format=serialization.PrivateFormat.PKCS8,
        encryption_algorithm=serialization.NoEncryption(),
    )


def get_public_key_pem() -> bytes:
    key = get_signing_key()
    return key.public_key().public_bytes(
        encoding=serialization.Encoding.PEM,
        format=serialization.PublicFormat.SubjectPublicKeyInfo,
    )
