<?php

namespace App\Application\Orders\DTOs;

class UpdateOrderDTO
{
    /** @param OrderItemDTO[]|null $items When provided, replaces the full item collection. */
    public function __construct(
        public readonly ?string $notes = null,
        public readonly ?int $customerId = null,
        public readonly ?array $items = null,
    ) {}
}
