from __future__ import annotations

import importlib.util
import sys
import tempfile
import unittest
from pathlib import Path


REPOSITORY_ROOT = Path(__file__).resolve().parents[2]
VALIDATOR_PATH = REPOSITORY_ROOT / "scripts" / "validate_repository.py"
SPEC = importlib.util.spec_from_file_location("gacha_pool_repository_validator", VALIDATOR_PATH)
assert SPEC and SPEC.loader
validator = importlib.util.module_from_spec(SPEC)
sys.modules[SPEC.name] = validator
SPEC.loader.exec_module(validator)


class RepositoryValidatorTests(unittest.TestCase):
    def test_schema_one_manifest_is_rejected(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            findings = validator.validate_repository_contract(
                root,
                {"schemaVersion": 1},
                effective_package_version=None,
            )

        self.assertEqual({"MTR002"}, {finding.code for finding in findings})

    def test_current_repository_contract_is_schema_two_and_valid(self) -> None:
        contract = validator.load_repository_contract(REPOSITORY_ROOT)
        self.assertIsNotNone(contract)
        self.assertEqual(2, contract["schemaVersion"])

        findings = validator.validate_repository_contract(
            REPOSITORY_ROOT,
            contract,
            effective_package_version=None,
        )
        projects = validator.find_packable_projects(REPOSITORY_ROOT)
        findings.extend(
            finding
            for project in projects
            for finding in validator.validate_project(REPOSITORY_ROOT, project, None)
        )

        errors = [finding for finding in findings if finding.code != "OK"]
        self.assertEqual([], errors)

    def test_typed_module_declarations_match_the_manifest(self) -> None:
        project = (
            REPOSITORY_ROOT
            / "src"
            / "Tairitsua.Monica.GachaPool"
            / "Tairitsua.Monica.GachaPool.csproj"
        )
        declarations = {
            declaration.class_name: declaration
            for declaration in validator.find_module_declarations(project)
        }

        self.assertEqual({"ModuleGachaPool", "ModuleGachaPoolUI"}, set(declarations))
        self.assertEqual(
            (("ModuleGachaPool", "ModuleGachaPoolOption"),),
            declarations["ModuleGachaPoolUI"].dependencies,
        )
        self.assertTrue(declarations["ModuleGachaPoolUI"].implements_ui)


if __name__ == "__main__":
    unittest.main()
