.PHONY: up down test test-backend test-mobile build

up:
	docker compose -f infra/docker-compose.yml up -d

down:
	docker compose -f infra/docker-compose.yml down

test: test-backend test-mobile

test-backend:
	dotnet test

test-mobile:
	cd mobile && npm test

build:
	dotnet build
