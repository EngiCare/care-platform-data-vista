# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Token service configuration — mirrors TokenServiceConfiguration.cs."""


class TokenServiceConfiguration:
    certificate_name: str = ""
    certificate_file_name: str = ""
    certificate_password: str = ""
    issuer_name: str = ""
    sts_base_address: str = ""
    sts_issue_address: str = ""
    realm: str = ""

    @classmethod
    def load(cls, settings) -> None:
        """Load from an AppSettings.token_service instance."""
        cls.certificate_name = settings.certificate_name
        cls.certificate_file_name = settings.certificate_file_name
        cls.certificate_password = settings.certificate_password
        cls.issuer_name = settings.issuer_name
        cls.sts_base_address = settings.sts_base_address
        cls.sts_issue_address = settings.sts_base_address + "Issue/"
        cls.realm = settings.audience
