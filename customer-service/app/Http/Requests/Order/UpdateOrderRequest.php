<?php

namespace App\Http\Requests\Order;

use Illuminate\Foundation\Http\FormRequest;

class UpdateOrderRequest extends FormRequest
{
    public function authorize(): bool
    {
        return true;
    }

    public function rules(): array
    {
        return [
            'notes'                   => ['sometimes', 'nullable', 'string', 'max:500'],
            'customer_id'             => ['sometimes', 'integer', 'min:1'],
            'items'                   => ['sometimes', 'array', 'min:1'],
            'items.*.description'     => ['required_with:items', 'string', 'max:300'],
            'items.*.quantity'        => ['required_with:items', 'integer', 'min:1'],
            'items.*.unit_price'      => ['required_with:items', 'numeric', 'min:0'],
        ];
    }
}
